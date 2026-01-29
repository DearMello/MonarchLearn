using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Admin;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ILogger<AdminService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            _logger.LogInformation("Generating admin dashboard statistics");

          
            var allUsers = await _unitOfWork.AppUsers.FindAsync(u => !u.IsDeleted);

            var studentCount = 0;
            var instructorCount = 0;

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student")) studentCount++;
                if (roles.Contains("Instructor")) instructorCount++;
            }

            
            var allCourses = await _unitOfWork.Courses.GetAllAsync(); 
            var totalCourses = allCourses.Count;

            
            var allEnrollments = await _unitOfWork.Enrollments.GetAllAsync();
            var totalEnrollments = allEnrollments.Count;
            var completedEnrollments = allEnrollments.Count(e => e.IsCompleted);
            var completionRate = totalEnrollments > 0
                ? Math.Round((double)completedEnrollments / totalEnrollments * 100, 2)
                : 0;

            
            var allAttempts = await _unitOfWork.Attempts.GetAllAsync();
            var totalAttempts = allAttempts.Count;
            var failedAttempts = allAttempts.Count(a => !a.IsPassed);
            var quizFailRate = totalAttempts > 0
                ? Math.Round((double)failedAttempts / totalAttempts * 100, 2)
                : 0;

            
            var topCourses = allCourses
                .OrderByDescending(c => c.Statistics != null ? c.Statistics.PopularityScore : 0)
                .Take(5)
                .Select(c => new TopCourseDto
                {
                    CourseTitle = c.Title,
                    EnrollmentCount = allEnrollments.Count(e => e.CourseId == c.Id),
                    PopularityScore = c.Statistics?.PopularityScore ?? 0
                })
                .ToList();

            
            var abandonedCourses = allCourses
                .Select(c => new
                {
                    Course = c,
                    EnrolledCount = allEnrollments.Count(e => e.CourseId == c.Id),
                    CompletedCount = allEnrollments.Count(e => e.CourseId == c.Id && e.IsCompleted)
                })
                .Where(x => x.EnrolledCount > 0) // Enrollment olmayan kursları çıxarırıq
                .Select(x => new AbandonedCourseDto
                {
                    CourseTitle = x.Course.Title,
                    EnrolledCount = x.EnrolledCount,
                    CompletedCount = x.CompletedCount,
                    AbandonmentRate = Math.Round(
                        (double)(x.EnrolledCount - x.CompletedCount) / x.EnrolledCount * 100, 2)
                })
                .OrderByDescending(x => x.AbandonmentRate)
                .Take(5)
                .ToList();

            _logger.LogInformation(
                "Dashboard stats generated: {Students} students, {Courses} courses, {Enrollments} enrollments",
                studentCount, totalCourses, totalEnrollments);

            return new AdminDashboardDto
            {
                TotalStudents = studentCount,
                TotalInstructors = instructorCount,
                TotalCourses = totalCourses,
                ActiveCourses = totalCourses, // Soft delete filter var
                TotalEnrollments = totalEnrollments,
                CompletedEnrollments = completedEnrollments,
                CompletionRate = completionRate,
                TotalQuizAttempts = totalAttempts,
                FailedQuizAttempts = failedAttempts,
                QuizFailRate = quizFailRate,
                TopCourses = topCourses,
                MostAbandonedCourses = abandonedCourses
            };
        }
    }
}
