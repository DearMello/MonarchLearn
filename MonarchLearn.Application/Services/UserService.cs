using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Users;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IFileService _fileService;

        public UserService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<UserService> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            _logger.LogInformation("Fetching user profile for User {UserId}", userId);

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.Educations)
                    .Include(u => u.WorkExperiences)
                    .Include(u => u.Streaks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) throw new NotFoundException("User", userId);
                if (user.IsDeleted) throw new ForbiddenException("Your account has been deactivated");

                var profile = _mapper.Map<UserProfileDto>(user);
                profile.ProfileImageUrl = _fileService.GetFullUrl(user.ProfileImageUrl);

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Instructor"))
                {
                    _logger.LogInformation("User {UserId} is an Instructor. Hiding student-specific data.", userId);
                    profile.CurrentStreak = null;
                    profile.CompletedCourses = null;
                }
                else
                {
                    var userStreak = user.Streaks.FirstOrDefault();
                    profile.CurrentStreak = userStreak?.CurrentStreakDays ?? 0;

                    var completedEnrollments = await _unitOfWork.Enrollments
                        .FindAsync(e => e.UserId == userId && e.IsCompleted && !e.IsDeleted);

                    profile.CompletedCourses = new List<CompletedCourseDto>();

                    foreach (var enrollment in completedEnrollments)
                    {
                        var course = await _unitOfWork.Courses.GetByIdAsync(enrollment.CourseId);
                        if (course == null || course.IsDeleted) continue;

                        var attempts = await _unitOfWork.Attempts.FindAsync(
                            a => a.EnrollmentId == enrollment.Id && a.IsPassed);

                        double averageGrade = 0;
                        if (attempts.Any())
                        {
                            var bestAttempts = attempts
                                .GroupBy(a => a.QuizId)
                                .Select(g => g.OrderByDescending(a => a.Percentage).First())
                                .ToList();

                            if (bestAttempts.Any())
                            {
                                averageGrade = Math.Round(bestAttempts.Average(a => a.Percentage), 2);
                            }
                        }

                        profile.CompletedCourses.Add(new CompletedCourseDto
                        {
                            CourseId = course.Id,
                            CourseName = course.Title,
                            CompletedAt = enrollment.UpdatedAt ?? enrollment.CreatedAt,
                            AverageGrade = averageGrade
                        });
                    }
                }

                return profile;
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                _logger.LogError(ex, "Failed to fetch profile for User {UserId}", userId);
                throw new BadRequestException("Failed to load user profile.");
            }
        }

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto model)
        {
            _logger.LogInformation("Updating profile for User {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted) throw new NotFoundException("User", userId);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (model.ProfileImage != null)
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                        _fileService.DeleteFile(user.ProfileImageUrl);

                    user.ProfileImageUrl = await _fileService.UploadFileAsync(model.ProfileImage, "profiles");
                }

                // ProfileMapping-dəki Condition sayəsində yalnız dolu gələn sahələr dəyişəcək
                _mapper.Map(model, user);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) throw new BadRequestException("Identity update failed");

                await transaction.CommitAsync();
                _logger.LogInformation("Profile updated successfully for User {UserId}", userId);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update profile for User {UserId}", userId);
                throw new BadRequestException("Failed to update profile.");
            }
        }

        public async Task<int> AddEducationAsync(int userId, UserEducationDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted) throw new NotFoundException("User", userId);

            if (model.GraduationDate.HasValue && model.StartDate > model.GraduationDate.Value)
                throw new BadRequestException("Start date cannot be after graduation date");

            var education = _mapper.Map<UserEducation>(model);
            education.UserId = userId;
            education.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserEducations.AddAsync(education);
            await _unitOfWork.SaveChangesAsync();

            return education.Id;
        }

        public async Task UpdateEducationAsync(int userId, int educationId, UserEducationDto model)
        {
            var education = await _unitOfWork.UserEducations.GetByIdAsync(educationId);
            if (education == null || education.UserId != userId) throw new NotFoundException("Education", educationId);

            _mapper.Map(model, education);
            education.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteEducationAsync(int userId, int educationId)
        {
            var education = await _unitOfWork.UserEducations.GetByIdAsync(educationId);
            if (education == null || education.UserId != userId) throw new NotFoundException("Education", educationId);

            _unitOfWork.UserEducations.Delete(education);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> AddWorkExperienceAsync(int userId, UserWorkExperienceDto model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted) throw new NotFoundException("User", userId);

            var work = _mapper.Map<UserWorkExperience>(model);
            work.UserId = userId;
            work.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserWorkExperiences.AddAsync(work);
            await _unitOfWork.SaveChangesAsync();

            return work.Id;
        }

        public async Task UpdateWorkExperienceAsync(int userId, int workId, UserWorkExperienceDto model)
        {
            var work = await _unitOfWork.UserWorkExperiences.GetByIdAsync(workId);
            if (work == null || work.UserId != userId) throw new NotFoundException("Work experience", workId);

            _mapper.Map(model, work);
            work.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteWorkExperienceAsync(int userId, int workId)
        {
            var work = await _unitOfWork.UserWorkExperiences.GetByIdAsync(workId);
            if (work == null || work.UserId != userId) throw new NotFoundException("Work experience", workId);

            _unitOfWork.UserWorkExperiences.Delete(work);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}