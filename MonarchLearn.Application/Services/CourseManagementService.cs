using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.CourseAdmin;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class CourseManagementService : ICourseManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseManagementService> _logger;
        private readonly IFileService _fileService;

        public CourseManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CourseManagementService> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        private void EnsureAccess(int ownerId, int currentUserId, bool isAdmin)
        {
            if (!isAdmin && ownerId != currentUserId)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access resource owned by {OwnerId}", currentUserId, ownerId);
                throw new ForbiddenException("You do not have permission to perform this action");
            }
        }

        public async Task<int> CreateCourseAsync(int userId, CreateCourseDto model)
        {
            _logger.LogInformation("Course creation requested by User {UserId}: {Title}", userId, model.Title);

            var instructor = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (instructor == null)
            {
                _logger.LogWarning("Course creation failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            await ValidateCourseReferencesAsync(model.CategoryId, model.LevelId, model.LanguageId);

            List<Skill>? skills = null;
            if (model.SkillIds != null && model.SkillIds.Any())
            {
                skills = await ValidateSkillsAsync(model.SkillIds);
            }

            var existingCourses = await _unitOfWork.Courses.FindAsync(
                c => c.Title == model.Title && c.InstructorId == userId && !c.IsDeleted);

            if (existingCourses.Any())
            {
                _logger.LogWarning("Course creation failed: User {UserId} already has a course titled '{Title}'", userId, model.Title);
                throw new ConflictException($"You already have a course with the title '{model.Title}'");
            }

            string? courseImageUrl = null;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (model.CourseImage != null)
                {
                    courseImageUrl = await _fileService.UploadFileAsync(model.CourseImage, "courses");
                    _logger.LogInformation("Course image uploaded: {ImageUrl}", courseImageUrl);
                }

                var course = _mapper.Map<Course>(model);
                course.InstructorId = userId;
                course.CourseImageUrl = courseImageUrl;
                course.CreatedAt = DateTime.UtcNow;
                course.IsDeleted = false;
                course.TotalDurationSeconds = 0;
                course.AverageRating = 0;
                course.ReviewCount = 0;

                if (skills != null && skills.Any())
                {
                    course.Skills = skills;
                    _logger.LogInformation("Course will be created with {Count} skill(s)", skills.Count);
                }

                course.Statistics = new CourseStatistics
                {
                    ViewCount = 0,
                    PopularityScore = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Course created successfully: ID {CourseId}, Title '{Title}' by User {UserId}",
                    course.Id, course.Title, userId);

                return course.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(courseImageUrl))
                {
                    try
                    {
                        _fileService.DeleteFile(courseImageUrl);
                        _logger.LogInformation("Uploaded image cleaned up after error: {ImageUrl}", courseImageUrl);
                    }
                    catch
                    {
                    }
                }

                _logger.LogError(ex, "Course creation failed for User {UserId}. Transaction rolled back", userId);
                throw new BadRequestException("Failed to create course. Please try again.");
            }
        }

        public async Task UpdateCourseAsync(int userId, UpdateCourseDto model, bool isAdmin = false)
        {
            _logger.LogInformation("Course update requested: ID {CourseId} by User {UserId} (IsAdmin: {IsAdmin})", model.Id, userId, isAdmin);

            var course = await _unitOfWork.Courses.GetCourseWithFullContentAsync(model.Id);
            if (course == null)
            {
                _logger.LogWarning("Update failed: Course {CourseId} not found", model.Id);
                throw new NotFoundException("Course", model.Id);
            }

            EnsureAccess(course.InstructorId, userId, isAdmin);

            await ValidateCourseReferencesAsync(
                model.CategoryId ?? course.CategoryId,
                model.LevelId ?? course.LevelId,
                model.LanguageId ?? course.LanguageId);

            if (model.SkillIds != null)
            {
                var skills = await ValidateSkillsAsync(model.SkillIds);
                course.Skills.Clear();
                foreach (var skill in skills)
                {
                    course.Skills.Add(skill);
                }
                _logger.LogInformation("Course {CourseId} skills updated: {Count} skill(s)", course.Id, skills.Count);
            }

            var duplicateCourses = await _unitOfWork.Courses.FindAsync(
                c => c.Title == model.Title && c.InstructorId == course.InstructorId && c.Id != model.Id && !c.IsDeleted);

            if (duplicateCourses.Any())
            {
                _logger.LogWarning("Update failed: Owner {OwnerId} already has another course titled '{Title}'", course.InstructorId, model.Title);
                throw new ConflictException($"Another course with the title '{model.Title}' already exists for this instructor");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (model.CourseImage != null)
                {
                    if (!string.IsNullOrEmpty(course.CourseImageUrl))
                    {
                        _fileService.DeleteFile(course.CourseImageUrl);
                        _logger.LogInformation("Old course image deleted: {OldUrl}", course.CourseImageUrl);
                    }

                    course.CourseImageUrl = await _fileService.UploadFileAsync(model.CourseImage, "courses");
                    _logger.LogInformation("New course image uploaded: {NewUrl}", course.CourseImageUrl);
                }

                string oldTitle = course.Title;
                _mapper.Map(model, course);
                course.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Course {CourseId} updated successfully: '{OldTitle}' -> '{NewTitle}'",
                    model.Id, oldTitle, course.Title);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Course update failed for Course {CourseId}. Transaction rolled back", model.Id);
                throw new BadRequestException("Failed to update course. Please try again.");
            }
        }

        public async Task DeleteCourseAsync(int userId, int courseId, bool isAdmin = false)
        {
            _logger.LogInformation("Course deletion requested: ID {CourseId} by User {UserId} (IsAdmin: {IsAdmin})", courseId, userId, isAdmin);

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Delete failed: Course {CourseId} not found", courseId);
                throw new NotFoundException("Course", courseId);
            }

            EnsureAccess(course.InstructorId, userId, isAdmin);

            var activeEnrollments = await _unitOfWork.Enrollments.FindAsync(
                e => e.CourseId == courseId && !e.IsDeleted);

            if (activeEnrollments.Any())
            {
                _logger.LogWarning("Delete failed: Course {CourseId} has {Count} active enrollment(s)", courseId, activeEnrollments.Count);
                throw new ConflictException($"Cannot delete this course because {activeEnrollments.Count} student(s) are enrolled.");
            }

            string courseTitle = course.Title;
            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("Course soft-deleted: ID {CourseId}, Title '{Title}' by User {UserId}", courseId, courseTitle, userId);
        }

        public async Task<List<CourseCardDto>> GetInstructorCoursesAsync(int userId)
        {
            _logger.LogDebug("Fetching courses for User {UserId}", userId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            var courses = await _unitOfWork.Courses.GetInstructorCoursesAsync(userId);

            _logger.LogInformation("Retrieved {Count} course(s) for User {UserId}", courses.Count, userId);

            var dtos = _mapper.Map<List<CourseCardDto>>(courses);

            foreach (var dto in dtos)
            {
                dto.ImageUrl = _fileService.GetFullUrl(dto.ImageUrl);
            }

            return dtos;
        }

        private async Task ValidateCourseReferencesAsync(int categoryId, int levelId, int languageId)
        {
            var category = await _unitOfWork.CourseCategories.GetByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Invalid CategoryId: {CategoryId}", categoryId);
                throw new NotFoundException("Category", categoryId);
            }

            var level = await _unitOfWork.CourseLevels.GetByIdAsync(levelId);
            if (level == null)
            {
                _logger.LogWarning("Invalid LevelId: {LevelId}", levelId);
                throw new NotFoundException("Level", levelId);
            }

            var language = await _unitOfWork.CourseLanguages.GetByIdAsync(languageId);
            if (language == null)
            {
                _logger.LogWarning("Invalid LanguageId: {LanguageId}", languageId);
                throw new NotFoundException("Language", languageId);
            }
        }

        private async Task<List<Skill>> ValidateSkillsAsync(List<int> skillIds)
        {
            if (skillIds == null || !skillIds.Any())
                return new List<Skill>();

            var skills = await _unitOfWork.Skills.FindAsync(s => skillIds.Contains(s.Id));

            if (skills.Count != skillIds.Count)
            {
                var foundIds = skills.Select(s => s.Id).ToList();
                var missingIds = skillIds.Except(foundIds).ToList();

                _logger.LogWarning("Invalid SkillIds: {MissingIds}", string.Join(", ", missingIds));
                throw new NotFoundException($"Skills not found: {string.Join(", ", missingIds)}");
            }

            return skills.ToList();
        }
    }
}