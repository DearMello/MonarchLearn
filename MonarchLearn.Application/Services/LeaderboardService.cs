using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Gamification;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Exceptions;
using System.Diagnostics;

namespace MonarchLearn.Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LeaderboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<LeaderboardUserDto>> GetGlobalLeaderboardAsync(int topCount)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Leaderboard generation started for top {Count} users", topCount);

            if (topCount <= 0)
            {
                _logger.LogWarning("Invalid topCount value: {TopCount}", topCount);
                throw new BadRequestException("Top count must be greater than 0");
            }

            if (topCount > 1000)
            {
                _logger.LogWarning("TopCount {TopCount} exceeds maximum allowed (1000)", topCount);
                throw new BadRequestException("Top count cannot exceed 1000");
            }

            try
            {
                // ✅ FIX: Get all users with their completed courses count and streak
                var users = await _unitOfWork.AppUsers.FindAsync(u => !u.IsDeleted);

                // Get all enrollments
                var allEnrollments = await _unitOfWork.Enrollments.FindAsync(e => !e.IsDeleted);

                // Get all streaks
                var allStreaks = await _unitOfWork.UserStreaks.GetAllAsync();

                var leaderboardData = users.Select(user =>
                {
                    var completedCoursesCount = allEnrollments
                        .Count(e => e.UserId == user.Id && e.IsCompleted);

                    var streak = allStreaks.FirstOrDefault(s => s.UserId == user.Id);

                    return new
                    {
                        User = user,
                        CompletedCoursesCount = completedCoursesCount,
                        CurrentStreakDays = streak?.CurrentStreakDays ?? 0,
                        Streak = streak
                    };
                })
                // ✅ FIX: Rank by completed courses first, then by streak
                .OrderByDescending(x => x.CompletedCoursesCount)
                .ThenByDescending(x => x.CurrentStreakDays)
                .Take(topCount)
                .ToList();

                _logger.LogDebug("Generated leaderboard data for {Count} users", leaderboardData.Count);

                if (!leaderboardData.Any())
                {
                    _logger.LogInformation("No user data available for leaderboard");
                    return new List<LeaderboardUserDto>();
                }

                // Get lesson completion counts for all users in leaderboard
                var userIds = leaderboardData.Select(x => x.User.Id).ToList();
                var completedLessons = await _unitOfWork.LessonProgresses
                    .FindAsync(lp => userIds.Contains(lp.Enrollment.UserId) && lp.IsCompleted);

                var leaderboard = new List<LeaderboardUserDto>();
                int rank = 1;

                foreach (var data in leaderboardData)
                {
                    var completedLessonsCount = completedLessons
                        .Where(lp => lp.Enrollment.UserId == data.User.Id)
                        .GroupBy(lp => lp.LessonItemId)
                        .Count();

                    leaderboard.Add(new LeaderboardUserDto
                    {
                        UserId = data.User.Id,
                        FullName = data.User.FullName,
                        ProfileImageUrl = data.User.ProfileImageUrl,
                        Rank = rank++,
                        CurrentStreakDays = data.CurrentStreakDays,
                        TotalCompletedLessons = completedLessonsCount,
                        CompletedCoursesCount = data.CompletedCoursesCount // ✅ NEW field
                    });
                }

                sw.Stop();
                _logger.LogInformation(
                    "Leaderboard generated successfully with {Count} users in {ElapsedMs}ms",
                    leaderboard.Count,
                    sw.ElapsedMilliseconds);

                return leaderboard;
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                sw.Stop();
                _logger.LogError(ex, "Failed to generate leaderboard after {ElapsedMs}ms", sw.ElapsedMilliseconds);
                throw new BadRequestException("Failed to load leaderboard. Please try again later.");
            }
        }

        public async Task<UserRankDto> GetUserRankAsync(int userId)
        {
            _logger.LogInformation("Calculating global rank for User {UserId}", userId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Rank calculation failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Rank calculation attempted for deleted user: {UserId}", userId);
                throw new ForbiddenException("Cannot retrieve rank for deactivated account");
            }

            try
            {
                // ✅ FIX: Calculate rank based on completed courses
                var userEnrollments = await _unitOfWork.Enrollments
                    .FindAsync(e => e.UserId == userId && !e.IsDeleted);

                var userCompletedCount = userEnrollments.Count(e => e.IsCompleted);

                // Get all users' completed course counts
                var allEnrollments = await _unitOfWork.Enrollments.FindAsync(e => !e.IsDeleted);
                var allStreaks = await _unitOfWork.UserStreaks.GetAllAsync();

                var allUsers = await _unitOfWork.AppUsers.FindAsync(u => !u.IsDeleted);

                var rankedUsers = allUsers.Select(u =>
                {
                    var completedCount = allEnrollments.Count(e => e.UserId == u.Id && e.IsCompleted);
                    var streak = allStreaks.FirstOrDefault(s => s.UserId == u.Id);

                    return new
                    {
                        UserId = u.Id,
                        CompletedCount = completedCount,
                        StreakDays = streak?.CurrentStreakDays ?? 0
                    };
                })
                .OrderByDescending(x => x.CompletedCount)
                .ThenByDescending(x => x.StreakDays)
                .ToList();

                int rank = rankedUsers.FindIndex(x => x.UserId == userId) + 1;

                var streaks = await _unitOfWork.UserStreaks.FindAsync(s => s.UserId == userId);
                var userStreak = streaks.FirstOrDefault();

                _logger.LogInformation(
                    "User {UserId} global rank is {Rank} with {CompletedCourses} completed courses and {Streak} days streak",
                    userId,
                    rank,
                    userCompletedCount,
                    userStreak?.CurrentStreakDays ?? 0);

                return new UserRankDto
                {
                    UserId = userId,
                    GlobalRank = rank,
                    CurrentStreakDays = userStreak?.CurrentStreakDays ?? 0
                };
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                _logger.LogError(ex, "Failed to calculate rank for User {UserId}", userId);
                throw new BadRequestException("Failed to retrieve user rank. Please try again later.");
            }
        }
    }
}