using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Enums;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class LessonCompletionService : ILessonCompletionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonCompletionService> _logger;
        private readonly ICertificateService _certificateService;

        public LessonCompletionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LessonCompletionService> logger,
            ICertificateService certificateService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _certificateService = certificateService;
        }

        public async Task<LessonProgressDto> GetLessonProgressForStudentAsync(int userId, int courseId, int lessonItemId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
            if (enrollment == null)
                throw new ForbiddenException("You must enroll in this course first");

            var progress = await _unitOfWork.LessonProgresses.GetProgressAsync(enrollment.Id, lessonItemId);

            if (progress == null)
            {
                return new LessonProgressDto
                {
                    LessonItemId = lessonItemId,
                    IsCompleted = false,
                    CompletedAt = null,
                    WatchedSeconds = null
                };
            }

            return _mapper.Map<LessonProgressDto>(progress);
        }

        public async Task<LessonProgressDto?> GetLessonProgressAsync(int enrollmentId, int lessonItemId)
        {
            _logger.LogDebug("Fetching progress for Enrollment {EnrollmentId}, Lesson {LessonId}",
                enrollmentId, lessonItemId);

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                _logger.LogWarning("Progress query failed: Enrollment {EnrollmentId} not found", enrollmentId);
                throw new NotFoundException("Enrollment", enrollmentId);
            }

            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(lessonItemId);
            if (lesson == null)
            {
                _logger.LogWarning("Progress query failed: Lesson {LessonId} not found", lessonItemId);
                throw new NotFoundException("Lesson", lessonItemId);
            }

            var progress = await _unitOfWork.LessonProgresses.GetProgressAsync(enrollmentId, lessonItemId);

            if (progress == null)
            {
                _logger.LogDebug("No progress found for Lesson {LessonId} (not started yet)", lessonItemId);
                return null;
            }

            return _mapper.Map<LessonProgressDto>(progress);
        }

        public async Task<bool> CanAccessLessonAsync(int userId, int courseId, int lessonItemId)
        {
            _logger.LogInformation(
                "Checking access rights: User {UserId} -> Lesson {LessonId} (Course {CourseId})",
                userId, lessonItemId, courseId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Access check failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Access check failed: Course {CourseId} not found", courseId);
                throw new NotFoundException("Course", courseId);
            }

            var enrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
            if (enrollment == null)
            {
                _logger.LogWarning("ACCESS DENIED: User {UserId} is NOT enrolled in Course {CourseId}",
                    userId, courseId);
                return false;
            }

            if (enrollment.IsDeleted)
            {
                _logger.LogWarning("ACCESS DENIED: Enrollment {EnrollmentId} is deleted", enrollment.Id);
                throw new ForbiddenException("Your enrollment has been deactivated");
            }

            var hasValidSubscription = await CheckSubscriptionAccessAsync(userId, courseId);
            if (!hasValidSubscription)
            {
                _logger.LogWarning("ACCESS DENIED: User {UserId} has no valid subscription for Course {CourseId}",
                    userId, courseId);
                throw new ForbiddenException("Your subscription has expired. Please renew to continue learning.");
            }

            var targetLesson = await _unitOfWork.LessonItems.GetByIdAsync(lessonItemId);
            if (targetLesson == null)
                throw new NotFoundException("Lesson", lessonItemId);

            if (targetLesson.IsDeleted)
                throw new BadRequestException("This lesson is no longer available");

            if (targetLesson.IsPreviewable)
            {
                _logger.LogInformation("Access Granted: Lesson {LessonId} is previewable", lessonItemId);
                return true;
            }

            if (targetLesson.Module == null)
            {
                targetLesson.Module = await _unitOfWork.Modules.GetByIdAsync(targetLesson.ModuleId);
                if (targetLesson.Module == null)
                    throw new NotFoundException("Module", targetLesson.ModuleId);
            }

            if (targetLesson.Module.CourseId != courseId)
                throw new BadRequestException("This lesson does not belong to the specified course");

            if (targetLesson.Module.Order == 1 && targetLesson.Order == 1)
            {
                _logger.LogInformation("Access Granted: First lesson of the course");
                return true;
            }

            int prevModuleId = targetLesson.ModuleId;
            int prevLessonOrder = targetLesson.Order - 1;

            if (targetLesson.Order == 1)
            {
                var prevModule = await _unitOfWork.Modules.FindAsync(
                    m => m.CourseId == courseId && m.Order == targetLesson.Module.Order - 1);

                if (!prevModule.Any())
                    return true;

                prevModuleId = prevModule.First().Id;

                var lastLessonOfPrevModule = (await _unitOfWork.LessonItems.FindAsync(
                    l => l.ModuleId == prevModuleId))
                    .OrderByDescending(l => l.Order)
                    .FirstOrDefault();

                if (lastLessonOfPrevModule == null)
                    return true;

                prevLessonOrder = lastLessonOfPrevModule.Order;
            }

            var previousLessonList = await _unitOfWork.LessonItems.FindAsync(
                l => l.ModuleId == prevModuleId && l.Order == prevLessonOrder);

            if (!previousLessonList.Any())
                return true;

            var actualPrevLessonId = previousLessonList.First().Id;
            var prevProgress = await _unitOfWork.LessonProgresses.GetProgressAsync(
                enrollment.Id, actualPrevLessonId);

            if (prevProgress == null || !prevProgress.IsCompleted)
            {
                _logger.LogWarning("ACCESS DENIED: User {UserId} tried to skip ahead.", userId);
                return false;
            }

            return true;
        }

        public async Task CompleteLessonAsync(int userId, int courseId, int lessonItemId, int watchedSeconds = 0, bool markAsFinished = false)
        {
            _logger.LogInformation("ACTION: Completing Lesson {LessonId} for User {UserId}. Manual Trigger: {MarkAsFinished}",
                lessonItemId, userId, markAsFinished);

            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(lessonItemId);
            if (lesson == null)
                throw new NotFoundException("Lesson", lessonItemId);

            if (lesson.LessonType == LessonType.Video)
            {
                int requiredSeconds;

                if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)
                {
                    requiredSeconds = lesson.VideoDurationSeconds.Value;
                }
                else if (lesson.EstimatedMinutes.HasValue && lesson.EstimatedMinutes.Value > 0)
                {
                    requiredSeconds = lesson.EstimatedMinutes.Value * 60;
                }
                else
                {
                    _logger.LogWarning("REJECTED: Video Lesson {LessonId} has no valid duration set", lessonItemId);
                    throw new BadRequestException("Video lesson must have a valid duration set by the instructor.");
                }

                int minimumWatchTime = (int)(requiredSeconds * 0.9);

                if (watchedSeconds < minimumWatchTime)
                {
                    _logger.LogWarning("ATTEMPT REJECTED: Video {LessonId} watched only {Watched}s of required {Minimum}s",
                        lessonItemId, watchedSeconds, minimumWatchTime);
                    throw new BadRequestException($"Videonun ən azı 90%-nə ({minimumWatchTime} saniyə) baxmalısınız.");
                }
            }
            else if (lesson.LessonType == LessonType.Reading)
            {
                if (!markAsFinished)
                {
                    _logger.LogWarning("REJECTED: Reading Lesson {LessonId} was not manually marked as finished", lessonItemId);
                    return;
                }
            }

            var enrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
            if (enrollment == null || enrollment.IsDeleted)
                throw new ForbiddenException("Enrollment not found or deactivated");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var lessonProgresses = await _unitOfWork.LessonProgresses.FindAsync(
                    lp => lp.EnrollmentId == enrollment.Id && lp.LessonItemId == lessonItemId);

                var progress = lessonProgresses.FirstOrDefault();

                if (progress == null)
                {
                    _logger.LogInformation("Creating NEW progress record for Lesson {LessonId}", lessonItemId);
                    progress = new LessonProgress
                    {
                        EnrollmentId = enrollment.Id,
                        LessonItemId = lessonItemId,
                        IsCompleted = true,
                        CompletedAt = DateTime.UtcNow,
                        WatchedSeconds = watchedSeconds,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.LessonProgresses.AddAsync(progress);
                }
                else
                {
                    _logger.LogInformation("Updating EXISTING progress record for Lesson {LessonId}", lessonItemId);
                    progress.IsCompleted = true;
                    progress.CompletedAt = DateTime.UtcNow;
                    progress.WatchedSeconds = watchedSeconds;
                    progress.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.LessonProgresses.Update(progress);
                }

                await _unitOfWork.SaveChangesAsync();

                enrollment.LastLessonItemId = lessonItemId;
                double calculatedPercent = await GetProgressPercentAsync(enrollment.Id, enrollment.CourseId);
                enrollment.ProgressPercent = (int)Math.Round(calculatedPercent);

                if (enrollment.ProgressPercent >= 100 && !enrollment.IsCompleted)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = DateTime.UtcNow;
                    await _certificateService.GenerateCertificateAsync(userId, enrollment.Id);
                    _logger.LogInformation("COURSE COMPLETED! Certificate generated for User {UserId}", userId);
                }

                var streaks = await _unitOfWork.UserStreaks.FindAsync(s => s.UserId == userId);
                var streak = streaks.FirstOrDefault();

                if (streak == null)
                {
                    _logger.LogInformation("Creating initial streak for User {UserId}", userId);
                    streak = new UserStreak
                    {
                        UserId = userId,
                        CurrentStreakDays = 1,
                        LastActiveDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.UserStreaks.AddAsync(streak);
                }
                else
                {
                    var bakuTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Azerbaijan Standard Time");
                    var nowBaku = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bakuTimeZone);
                    var lastActiveBaku = TimeZoneInfo.ConvertTimeFromUtc(streak.LastActiveDate, bakuTimeZone);

                    if (lastActiveBaku.Date < nowBaku.Date)
                    {
                        if (lastActiveBaku.Date == nowBaku.Date.AddDays(-1))
                        {
                            streak.CurrentStreakDays += 1;
                        }
                        else
                        {
                            streak.CurrentStreakDays = 1;
                        }

                        streak.LastActiveDate = DateTime.UtcNow;
                        _unitOfWork.UserStreaks.Update(streak);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                _logger.LogInformation("Database changes saved successfully for User {UserId}", userId);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                _logger.LogError(ex, "Failed to complete Lesson {LessonId}", lessonItemId);
                throw;
            }
        }

        public async Task<double> GetProgressPercentAsync(int enrollmentId, int? courseId = null)
        {
            if (courseId == null)
            {
                var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
                courseId = enrollment?.CourseId ?? 0;
            }

            var allLessons = await _unitOfWork.LessonItems.FindAsync(
                l => l.Module.CourseId == courseId.Value && !l.IsDeleted);
            var totalCount = allLessons.Count;

            if (totalCount == 0)
                return 0;

            var progressList = await _unitOfWork.LessonProgresses.FindAsync(
                lp => lp.EnrollmentId == enrollmentId && lp.IsCompleted);
            var completedCount = progressList.Count;

            double percent = ((double)completedCount / totalCount) * 100;
            return Math.Round(percent, 2);
        }

        private async Task<bool> CheckSubscriptionAccessAsync(int userId, int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return false;

            var userSubscriptions = await _unitOfWork.UserSubscriptions.FindAsync(
                s => s.UserId == userId && !s.IsDeleted);

            if (!userSubscriptions.Any())
                return false;

            var activeSubscription = userSubscriptions
                .Where(s => s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            if (activeSubscription == null)
                return false;

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(activeSubscription.SubscriptionPlanId);
            if (plan == null)
                return false;

            if (plan.Name.Contains("Free Trial", StringComparison.OrdinalIgnoreCase))
            {
                var freeTrialEnrollments = await _unitOfWork.Enrollments.FindAsync(
                    e => e.UserId == userId && !e.IsDeleted);

                if (freeTrialEnrollments.Count() >= 1)
                {
                    var currentEnrollment = freeTrialEnrollments.FirstOrDefault(e => e.CourseId == courseId);
                    if (currentEnrollment == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}