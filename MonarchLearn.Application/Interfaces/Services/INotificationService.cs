namespace MonarchLearn.Application.Interfaces.Services
{
    public interface INotificationService
    {
        // İstifadədə olan metod
        Task SendNotificationAsync(int userId, string subject, string body);

        // Hangfire tərəfindən arxa planda icra edilən metod
        Task ExecuteEmailSendAsync(int userId, string subject, string body);
    }
}