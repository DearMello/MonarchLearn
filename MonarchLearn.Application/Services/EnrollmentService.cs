using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Enrollment;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Enrollments;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly UserManager<AppUser> _userManager;

        private static readonly SemaphoreSlim _enrollmentLock = new SemaphoreSlim(1, 1);

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EnrollmentService> logger, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Enrollment?> GetUserEnrollmentAsync(int userId, int courseId)
        {
            return await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
        }

        public async Task<object> GetCourseProgressAsync(int userId, int courseId)
        {
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null) throw new ForbiddenException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");
            var isInstructor = userRoles.Contains("Instructor");

           
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) throw new NotFoundException("Course", courseId);

        
            var enrollment = await GetUserEnrollmentAsync(userId, courseId);

         
            bool hasAccess = enrollment != null ||
                             isAdmin ||
                             (isInstructor && course.InstructorId == userId);

            if (!hasAccess)
                throw new ForbiddenException("Bu kursun tərkibini görmək üçün icazəniz yoxdur.");

            var modules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == courseId);
            var moduleList = modules.OrderBy(m => m.Order).ToList();

            var progressData = new List<object>();

            foreach (var module in moduleList)
            {
                var lessons = await _unitOfWork.LessonItems.FindAsync(l => l.ModuleId == module.Id);
                var lessonList = lessons.OrderBy(l => l.Order).ToList();
                var lessonProgresses = new List<object>();

                foreach (var lesson in lessonList)
                {
                    var isCompleted = false;
                    DateTime? completedAt = null;

                    if (enrollment != null)
                    {
                        var progress = await _unitOfWork.LessonProgresses.GetProgressAsync(enrollment.Id, lesson.Id);
                        isCompleted = progress?.IsCompleted ?? false;
                        completedAt = progress?.CompletedAt;
                    }

                    lessonProgresses.Add(new
                    {
                        lessonId = lesson.Id,
                        title = lesson.Title,
                        type = lesson.LessonType.ToString(),
                        isCompleted = isCompleted,
                        completedAt = completedAt
                    });
                }

                progressData.Add(new
                {
                    moduleId = module.Id,
                    title = module.Title,
                    order = module.Order,
                    lessons = lessonProgresses
                });
            }

            return new
            {
                courseId = courseId,
                enrollmentId = enrollment?.Id ?? 0,
                progressPercent = enrollment?.ProgressPercent ?? 0,
                isCompleted = enrollment?.IsCompleted ?? false,
                modules = progressData
            };
        }

        public async Task<EnrollmentDto> EnrollStudentAsync(int userId, int courseId)
        {
            _logger.LogInformation("Enrollment request: User {UserId} -> Course {CourseId}", userId, courseId);

            await _enrollmentLock.WaitAsync();
            try
            {
                var existingEnrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
                if (existingEnrollment != null && !existingEnrollment.IsDeleted)
                {
                    _logger.LogInformation("User {UserId} is already enrolled in Course {CourseId}.", userId, courseId);

                    var dto = _mapper.Map<EnrollmentDto>(existingEnrollment);
                    if (existingEnrollment.IsCompleted)
                    {
                        dto.AverageGrade = await CalculateAverageGradeAsync(existingEnrollment.Id);
                    }
                    return dto;
                }

                var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    throw new ForbiddenException("Account is inactive or not found.");
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("ENROLLMENT DENIED: User {UserId} has not verified their email", userId);
                    throw new ForbiddenException("Please verify your email before enrolling in courses.");
                }

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null || course.IsDeleted)
                {
                    throw new NotFoundException("Course not found or unavailable.");
                }

                var activeSub = await _unitOfWork.UserSubscriptions.GetActiveSubscriptionAsync(userId);
                if (activeSub == null)
                {
                    _logger.LogWarning("ENROLLMENT DENIED: User {UserId} has no active subscription", userId);
                    throw new ForbiddenException("You need an active subscription to enroll in courses.");
                }

                var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(activeSub.SubscriptionPlanId);
                //if (plan != null && plan.Price == 0)
                //{
                //    _logger.LogDebug("User {UserId} has free trial subscription", userId);
                //    var userEnrollments = await _unitOfWork.Enrollments.FindAsync(e => e.UserId == userId && !e.IsDeleted);
                //    if (userEnrollments.Any())
                //    {
                //        _logger.LogWarning("ENROLLMENT DENIED: User {UserId} already has {Count} enrollment(s) with free trial", userId, userEnrollments.Count());
                //        throw new ForbiddenException("Free trial allows enrollment in only 1 course.");
                //    }
                //}

                _logger.LogInformation("Validations passed for enrollment: User {UserId}, Email verified, Subscription active until {EndDate}", userId, activeSub.EndDate);

                return await CreateEnrollmentInternalAsync(userId, courseId, course.Title);
            }
            finally
            {
                _enrollmentLock.Release();
            }
        }

        public async Task<List<EnrollmentDto>> GetStudentEnrollmentsAsync(int userId)
        {
            _logger.LogDebug("Fetching enrollments for User {UserId}", userId);
            var enrollments = await _unitOfWork.Enrollments.GetUserEnrollmentsAsync(userId);
            var enrollmentDtos = _mapper.Map<List<EnrollmentDto>>(enrollments);

            foreach (var dto in enrollmentDtos.Where(e => e.IsCompleted))
            {
                var enrollment = enrollments.FirstOrDefault(e => e.Id == dto.Id);
                if (enrollment != null)
                {
                    dto.AverageGrade = await CalculateAverageGradeAsync(enrollment.Id);
                }
            }
            return enrollmentDtos;
        }

        public async Task<int?> GetLastLessonIdAsync(int userId, int courseId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
            if (enrollment == null) throw new ForbiddenException("Enrollment required.");
            return enrollment.LastLessonItemId;
        }

        private async Task<double> CalculateAverageGradeAsync(int enrollmentId)
        {
            var attempts = await _unitOfWork.Attempts.FindAsync(a => a.EnrollmentId == enrollmentId && a.IsPassed);
            if (!attempts.Any()) return 0;

            var bestAttempts = attempts
                .GroupBy(a => a.QuizId)
                .Select(g => g.OrderByDescending(a => a.Percentage).First())
                .ToList();

            return bestAttempts.Any() ? Math.Round(bestAttempts.Average(a => a.Percentage), 2) : 0;
        }

        private async Task<EnrollmentDto> CreateEnrollmentInternalAsync(int userId, int courseId, string courseTitle)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    StartedAt = DateTime.UtcNow,
                    ProgressPercent = 0,
                    IsCompleted = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Enrollments.AddAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Enrollment successful: User {UserId} -> {CourseTitle}", userId, courseTitle);

                var createdEnrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
                return _mapper.Map<EnrollmentDto>(createdEnrollment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (ex.InnerException?.Message.Contains("UNIQUE") == true || ex.Message.Contains("duplicate"))
                {
                    _logger.LogWarning("Race condition detected: Duplicate enrollment attempt for User {UserId}", userId);
                    var existing = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, courseId);
                    return _mapper.Map<EnrollmentDto>(existing);
                }
                _logger.LogError(ex, "Enrollment failed for User {UserId}", userId);
                throw new BadRequestException("Enrollment failed. Please try again.");
            }
        }
    }
}