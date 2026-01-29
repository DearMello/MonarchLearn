using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using System.Net;
using System.Net.Mail;

namespace MonarchLearn.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public Task SendNotificationAsync(int userId, string subject, string body)
        {
            _logger.LogInformation("Enqueuing notification job for user {UserId}", userId);

           
            BackgroundJob.Enqueue<INotificationService>(x => x.ExecuteEmailSendAsync(userId, subject, body));

            return Task.CompletedTask;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteEmailSendAsync(int userId, string subject, string body)
        {
            try
            {
                var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("Notification aborted: User {UserId} not found or email is missing.", userId);
                    return;
                }

                var smtpSettings = _configuration.GetSection("EmailSettings");

               
                string host = smtpSettings["SmtpServer"] ?? throw new Exception("SMTP Server config is missing.");
                int port = int.Parse(smtpSettings["Port"] ?? "587");
                string senderEmail = smtpSettings["SenderEmail"] ?? throw new Exception("Sender Email config is missing.");
                string password = smtpSettings["Password"] ?? throw new Exception("Email Password config is missing.");

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, smtpSettings["SenderName"] ?? "Monarch Learn"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(user.Email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Background email successfully sent to {Email} for User {UserId}", user.Email, userId);
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Error occurred while sending background email to User {UserId}", userId);
                throw;
            }
        }
    }
}