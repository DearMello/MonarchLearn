using Microsoft.Extensions.Logging;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Infrastructure.BackgroundJobs
{
    public class ScheduledTaskService : IScheduledTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly INotificationService _notificationService;

        private static readonly TimeZoneInfo BakuTimeZone =
            TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == "Azerbaijan Standard Time")
                ? TimeZoneInfo.FindSystemTimeZoneById("Azerbaijan Standard Time") // Windows üçün
                : TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"); // Linux/Docker üçün

        public ScheduledTaskService(
            IUnitOfWork unitOfWork,
            ILogger<ScheduledTaskService> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task ResetInactiveStreaksAsync()
        {
            _logger.LogInformation("--- STREAK RESET JOB STARTED ---");

            var nowBaku = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BakuTimeZone);
            // FIX: yesterdayBaku artıq tam 24 saatlıq pəncərəni təmsil edir
            var yesterdayBaku = nowBaku.Date.AddDays(-1);

            _logger.LogInformation("Checking inactivity before Baku date: {Yesterday}", yesterdayBaku.ToShortDateString());

            var allStreaks = await _unitOfWork.UserStreaks.FindAsync(s => s.CurrentStreakDays > 0);

            // FIX: Müqayisəni elə qurduq ki, əgər son aktivlik dünən (14-ü) olubsa, sıfırlanmasın.
            // Yalnız son aktivliyi dünəndən də əvvəl (məs: 13-ü və daha köhnə) olanlar sıfırlanacaq.
            var inactiveStreaks = allStreaks.Where(s =>
                TimeZoneInfo.ConvertTimeFromUtc(s.LastActiveDate, BakuTimeZone).Date < yesterdayBaku.Date
            ).ToList();

            int resetCount = 0;

            foreach (var streak in inactiveStreaks)
            {
                try
                {
                    _logger.LogWarning("Resetting streak for User {UserId}. Last active (UTC): {Date}",
                        streak.UserId, streak.LastActiveDate);

                    streak.CurrentStreakDays = 0;
                    streak.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.UserStreaks.Update(streak);
                    resetCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reset streak for User {UserId}", streak.UserId);
                }
            }

            if (resetCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("--- STREAK RESET JOB FINISHED. Total {Count} users reset. ---", resetCount);
        }

        public async Task SendSubscriptionExpiryNotificationsAsync()
        {
            _logger.LogInformation("--- SUBSCRIPTION NOTIFICATION JOB STARTED ---");

            var nowBaku = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BakuTimeZone);
            var today = nowBaku.Date;
            var tomorrow = today.AddDays(1);

            var expiringToday = await _unitOfWork.UserSubscriptions.FindAsync(s => s.EndDate.Date == today);
            var expiringSoon = await _unitOfWork.UserSubscriptions.FindAsync(s => s.EndDate.Date == tomorrow);

            var notificationTasks = new List<Task>();

            foreach (var sub in expiringToday)
            {
                notificationTasks.Add(_notificationService.SendNotificationAsync(
                    sub.UserId,
                    "Last Chance! Subscription expires TODAY!",
                    "Dear student, renew your subscription now to avoid losing access to your courses."));
            }

            foreach (var sub in expiringSoon)
            {
                notificationTasks.Add(_notificationService.SendNotificationAsync(
                    sub.UserId,
                    "Subscription expires tomorrow.",
                    "Reminder: Your access will expire in 24 hours. Renew today!"));
            }

            if (notificationTasks.Any())
            {
                await Task.WhenAll(notificationTasks);
                _logger.LogInformation("Sent {Count} expiry notifications.", notificationTasks.Count);
            }

            _logger.LogInformation("--- SUBSCRIPTION NOTIFICATION JOB FINISHED ---");
        }
    }
}