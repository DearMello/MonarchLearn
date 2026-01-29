using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.DTOs.Modules;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ModuleService> _logger;

        public ModuleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ModuleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        private async Task ValidateCourseOwnershipAsync(int userId, int courseId, bool isAdmin)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) throw new NotFoundException("Course", courseId);
            if (course.IsDeleted) throw new BadRequestException("Course is deleted");

            if (!isAdmin && course.InstructorId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to manage modules for Course {CourseId} owned by {OwnerId}", userId, courseId, course.InstructorId);
                throw new ForbiddenException("You don't have permission to manage modules for this course.");
            }
        }

        public async Task<int> CreateModuleAsync(int userId, CreateModuleDto model, bool isAdmin = false)
        {
            _logger.LogInformation("Creating module '{Title}' for Course {CourseId} by User {UserId}", model.Title, model.CourseId, userId);

            await ValidateCourseOwnershipAsync(userId, model.CourseId, isAdmin);

            var existingModules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == model.CourseId && !m.IsDeleted);
            if (existingModules.Any(m => m.Title.Equals(model.Title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictException($"A module with the title '{model.Title}' already exists in this course");
            }

            int newOrder = existingModules.Any() ? existingModules.Max(m => m.Order) + 1 : 1;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var module = new Module
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Order = newOrder,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Modules.AddAsync(module);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return module.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create module");
                throw new BadRequestException("Failed to create module. Please try again.");
            }
        }

        public async Task UpdateModuleAsync(int userId, UpdateModuleDto model, bool isAdmin = false)
        {
            _logger.LogInformation("Updating module {ModuleId} by User {UserId}", model.Id, userId);

            var module = await _unitOfWork.Modules.GetByIdAsync(model.Id);
            if (module == null || module.IsDeleted)
                throw new NotFoundException("Module", model.Id);

            await ValidateCourseOwnershipAsync(userId, module.CourseId, isAdmin);

            // FIX: Duplicate yoxlamasını yalnız Title gəlibsə edirik
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                var duplicates = await _unitOfWork.Modules.FindAsync(m =>
                    m.CourseId == module.CourseId &&
                    m.Id != model.Id &&
                    !m.IsDeleted &&
                    m.Title.ToLower() == model.Title.ToLower());

                if (duplicates.Any())
                    throw new ConflictException("Module title already exists");
            }

            // FIX: Manual mənimsətmə yerinə sığortalanmış Mapper istifadə edirik
            _mapper.Map(model, module);
            module.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteModuleAsync(int userId, int moduleId, bool isAdmin = false)
        {
            _logger.LogInformation("Deleting module {ModuleId} by User {UserId}", moduleId, userId);

            var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);
            if (module == null || module.IsDeleted)
                throw new NotFoundException("Module", moduleId);

            await ValidateCourseOwnershipAsync(userId, module.CourseId, isAdmin);

            var lessons = await _unitOfWork.LessonItems.FindAsync(l => l.ModuleId == moduleId && !l.IsDeleted);
            if (lessons.Any())
                throw new ConflictException("Cannot delete module containing lessons");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                module.IsDeleted = true;
                module.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Modules.Update(module);
                await _unitOfWork.SaveChangesAsync();

                await RecalculateCourseDurationAsync(module.CourseId);

                await transaction.CommitAsync();

                _logger.LogInformation("Module {ModuleId} deleted and course duration recalculated", moduleId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete module {ModuleId}", moduleId);
                throw new BadRequestException("Failed to delete module. Please try again.");
            }
        }

        public async Task<List<ModuleWithLessonsDto>> GetModulesByCourseIdAsync(int courseId)
        {
            _logger.LogDebug("Fetching modules with lessons for Course {CourseId}", courseId);

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                throw new NotFoundException("Course", courseId);

            var modules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == courseId && !m.IsDeleted);
            if (!modules.Any())
                return new List<ModuleWithLessonsDto>();

            var moduleIds = modules.Select(m => m.Id).ToList();
            var allLessons = await _unitOfWork.LessonItems.FindAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);

            var orderedModules = modules.OrderBy(m => m.Order).ToList();
            var moduleDtos = _mapper.Map<List<ModuleWithLessonsDto>>(orderedModules);

            foreach (var moduleDto in moduleDtos)
            {
                var currentModuleLessons = allLessons
                    .Where(l => l.ModuleId == moduleDto.Id)
                    .OrderBy(l => l.Order)
                    .ToList();

                moduleDto.Lessons = _mapper.Map<List<LessonDetailDto>>(currentModuleLessons);
            }

            _logger.LogInformation("Successfully retrieved {Count} modules with mapped lessons", moduleDtos.Count);
            return moduleDtos;
        }

        private async Task RecalculateCourseDurationAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return;

            var modules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == courseId && !m.IsDeleted);
            if (!modules.Any())
            {
                course.TotalDurationSeconds = 0;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var moduleIds = modules.Select(m => m.Id).ToList();
            var lessons = await _unitOfWork.LessonItems.FindAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);

            int totalSeconds = 0;

            foreach (var lesson in lessons)
            {
                if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)
                {
                    totalSeconds += lesson.VideoDurationSeconds.Value;
                }
                else if (lesson.EstimatedMinutes.HasValue)
                {
                    totalSeconds += lesson.EstimatedMinutes.Value * 60;
                }
            }

            course.TotalDurationSeconds = totalSeconds;
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Course {CourseId} duration recalculated: {TotalSeconds} seconds ({Minutes} minutes)",
                courseId, totalSeconds, totalSeconds / 60);
        }
    }
}