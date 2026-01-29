using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Entities.Enums;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;
        private readonly IFileService _fileService;

        public LessonService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LessonService> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        private async Task ValidateModuleOwnershipAsync(int userId, int moduleId, bool isAdmin)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);
            if (module == null) throw new NotFoundException("Module", moduleId);

            var course = await _unitOfWork.Courses.GetByIdAsync(module.CourseId);
            if (course == null) throw new NotFoundException("Course", module.CourseId);

            if (!isAdmin && course.InstructorId != userId)
            {
                _logger.LogWarning("Unauthorized access: User {UserId} tried to access Module {ModuleId}", userId, moduleId);
                throw new ForbiddenException("You don't have permission to manage lessons in this course.");
            }
        }

        public async Task<CreateLessonResponseDto> CreateLessonAsync(int userId, CreateLessonDto model, bool isAdmin = false)
        {
            _logger.LogInformation("Creating new lesson in Module {ModuleId}: {Title} by User {UserId}", model.ModuleId, model.Title, userId);

            await ValidateModuleOwnershipAsync(userId, model.ModuleId, isAdmin);

            var module = await _unitOfWork.Modules.GetByIdAsync(model.ModuleId);

            string? videoUrl = null;
            int foundDuration = 0;

            if (model.VideoFile != null)
            {
                var result = await _fileService.UploadVideoWithDurationAsync(model.VideoFile);
                videoUrl = result.FilePath;
                foundDuration = result.Duration;
            }
            else if (!string.IsNullOrEmpty(model.VideoUrl))
            {
                videoUrl = model.VideoUrl;
            }

            var existingLessons = await _unitOfWork.LessonItems
                .FindAsync(l => l.ModuleId == model.ModuleId && !l.IsDeleted);

            int newOrder = existingLessons.Any() ? existingLessons.Max(l => l.Order) + 1 : 1;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var lesson = _mapper.Map<LessonItem>(model);
                lesson.VideoUrl = videoUrl;

                if (foundDuration > 0)
                {
                    lesson.VideoDurationSeconds = foundDuration;
                    lesson.EstimatedMinutes = (int)Math.Ceiling(foundDuration / 60.0);
                }
                else
                {
                    if (model.VideoDurationSeconds.HasValue && model.VideoDurationSeconds > 0)
                        lesson.VideoDurationSeconds = model.VideoDurationSeconds;

                    if (model.EstimatedMinutes.HasValue && model.EstimatedMinutes > 0)
                        lesson.EstimatedMinutes = model.EstimatedMinutes;
                    else if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds > 0)
                        lesson.EstimatedMinutes = (int)Math.Ceiling(lesson.VideoDurationSeconds.Value / 60.0);
                }

                if (model.LessonType == LessonType.Reading && !string.IsNullOrEmpty(model.ReadingText))
                {
                    if (lesson.EstimatedMinutes == null || lesson.EstimatedMinutes == 0)
                    {
                        int wordCount = model.ReadingText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        lesson.EstimatedMinutes = (int)Math.Ceiling(wordCount / 200.0);
                        if (lesson.EstimatedMinutes == 0) lesson.EstimatedMinutes = 1;
                    }
                }

                lesson.Order = newOrder;
                lesson.CreatedAt = DateTime.UtcNow;
                lesson.IsDeleted = false;

                await _unitOfWork.LessonItems.AddAsync(lesson);
                await _unitOfWork.SaveChangesAsync();

                int? autoCreatedQuizId = null;

                if (model.LessonType == LessonType.Quiz)
                {
                    var newQuiz = new Quiz
                    {
                        LessonItemId = lesson.Id,
                        Title = lesson.Title,
                        TimeLimitSeconds = 0,
                        PassingScorePercent = 50,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _unitOfWork.Quizzes.AddAsync(newQuiz);
                    await _unitOfWork.SaveChangesAsync();

                    autoCreatedQuizId = newQuiz.Id;
                    _logger.LogInformation("Automatic Quiz created with ID {QuizId} for Lesson {LessonId}", autoCreatedQuizId, lesson.Id);
                }

                await RecalculateCourseDurationAsync(module.CourseId);
                await transaction.CommitAsync();

                return new CreateLessonResponseDto
                {
                    LessonId = lesson.Id,
                    QuizId = autoCreatedQuizId,
                    Message = "Lesson and associated resources created successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (!string.IsNullOrEmpty(videoUrl) && model.VideoFile != null)
                {
                    _fileService.DeleteFile(videoUrl);
                }
                throw new BadRequestException("Failed to create lesson: " + ex.Message);
            }
        }

        public async Task UpdateLessonAsync(int userId, UpdateLessonDto model, bool isAdmin = false)
        {
            _logger.LogInformation("Updating lesson ID {LessonId} by User {UserId}", model.Id, userId);

            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(model.Id);
            if (lesson == null) throw new NotFoundException("Lesson", model.Id);
            if (lesson.IsDeleted) throw new BadRequestException("Cannot update a deleted lesson");

            await ValidateModuleOwnershipAsync(userId, lesson.ModuleId, isAdmin);

            var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                //  Yeni video faylı gələrsə köhnəni silirik
                if (model.VideoFile != null)
                {
                    if (!string.IsNullOrEmpty(lesson.VideoUrl))
                    {
                        _fileService.DeleteFile(lesson.VideoUrl);
                    }
                    var result = await _fileService.UploadVideoWithDurationAsync(model.VideoFile);
                    lesson.VideoUrl = result.FilePath;
                    lesson.VideoDurationSeconds = result.Duration;
                    lesson.EstimatedMinutes = (int)Math.Ceiling(result.Duration / 60.0);
                }

                _mapper.Map(model, lesson);

                if (lesson.LessonType == LessonType.Reading && !string.IsNullOrWhiteSpace(lesson.ReadingText))
                {
                    if (lesson.EstimatedMinutes == null || lesson.EstimatedMinutes == 0)
                    {
                        int wordCount = lesson.ReadingText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        lesson.EstimatedMinutes = (int)Math.Ceiling(wordCount / 200.0);
                        if (lesson.EstimatedMinutes == 0) lesson.EstimatedMinutes = 1;
                    }
                }

                lesson.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await RecalculateCourseDurationAsync(module.CourseId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Failed to update lesson: " + ex.Message);
            }
        }

        public async Task<bool> DeleteLessonAsync(int userId, int lessonId, bool isAdmin = false)
        {
            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(lessonId);
            if (lesson == null) throw new NotFoundException("Lesson", lessonId);

            await ValidateModuleOwnershipAsync(userId, lesson.ModuleId, isAdmin);

            var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId);
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                lesson.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                await RecalculateCourseDurationAsync(module.CourseId);
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<LessonDetailDto>> GetLessonsByModuleIdAsync(int moduleId)
        {
            var lessons = await _unitOfWork.LessonItems.FindAsync(l => l.ModuleId == moduleId && !l.IsDeleted);
            return _mapper.Map<List<LessonDetailDto>>(lessons.OrderBy(l => l.Order));
        }

        private async Task RecalculateCourseDurationAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return;

            var modules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == courseId && !m.IsDeleted);
            var moduleIds = modules.Select(m => m.Id).ToList();
            var allLessons = await _unitOfWork.LessonItems.FindAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);

            int totalSeconds = 0;
            foreach (var lesson in allLessons)
            {
                if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)
                    totalSeconds += lesson.VideoDurationSeconds.Value;
                else if (lesson.EstimatedMinutes.HasValue)
                    totalSeconds += lesson.EstimatedMinutes.Value * 60;
            }
            course.TotalDurationSeconds = totalSeconds;
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Course {CourseId} duration updated to {Seconds}s", courseId, totalSeconds);
        }

        private void CalculateReadingDuration(LessonItem lesson, string readingText)
        {
            if (!string.IsNullOrEmpty(readingText))
            {
                int wordCount = readingText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                lesson.EstimatedMinutes = (int)Math.Ceiling(wordCount / 200.0);
                if (lesson.EstimatedMinutes == 0) lesson.EstimatedMinutes = 1;
                _logger.LogInformation("Reading duration calculated: {Minutes} min", lesson.EstimatedMinutes);
            }
        }
    }
}