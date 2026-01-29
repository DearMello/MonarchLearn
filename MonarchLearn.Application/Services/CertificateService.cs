using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Certificates;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Exceptions;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CertificateService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public CertificateService(
            IUnitOfWork unitOfWork,
            ILogger<CertificateService> logger,
            IWebHostEnvironment env,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _env = env;
            _mapper = mapper;
        }

        public async Task<CertificateDto> GetCertificateByIdAsync(int userId, int certificateId)
        {
            var certificates = await _unitOfWork.Certificates.FindAsync(c => c.Id == certificateId);
            var certificate = certificates.FirstOrDefault();

            if (certificate == null)
                throw new NotFoundException("Certificate", certificateId);

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(certificate.EnrollmentId);
            if (enrollment == null || enrollment.UserId != userId)
                throw new ForbiddenException("You can only access your own certificates");

            var course = await _unitOfWork.Courses.GetByIdAsync(enrollment.CourseId);
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);

            string html = await PrepareHtmlContentAsync(
                user.FullName,
                course.Title,
                certificate.IssuedAt,
                certificate.Id.ToString());

            var dto = _mapper.Map<CertificateDto>(certificate);
            dto.CourseName = course.Title;
            dto.HtmlContent = html;
            dto.AverageGrade = await CalculateAverageGradeAsync(enrollment.Id);
            dto.StudentName = user.FullName;

            return dto;
        }

        public async Task<CertificateFileDto> GetCertificatePdfAsync(int userId, int certificateId)
        {
            var certificate = await GetCertificateByIdAsync(userId, certificateId);
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            var pdfPath = Path.Combine(_env.WebRootPath, certificate.PdfUrl.TrimStart('/'));

            if (user.FullName != certificate.StudentName && File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }

           
            if (!File.Exists(pdfPath))
            {
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
                });

                await using var page = await browser.NewPageAsync();
                
                await page.SetContentAsync(certificate.HtmlContent, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                await page.EmulateMediaTypeAsync(MediaType.Print);
                await page.PdfAsync(pdfPath, new PdfOptions { Format = PaperFormat.A4, PrintBackground = true });
            }

            var fileBytes = await File.ReadAllBytesAsync(pdfPath);
            return new CertificateFileDto
            {
                FileBytes = fileBytes,
                FileName = $"Certificate_{certificateId}.pdf"
            };
        }

        public async Task<CertificateDto> GenerateCertificateAsync(int userId, int enrollmentId)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null) throw new NotFoundException("Enrollment", enrollmentId);

            if (enrollment.UserId != userId)
                throw new ForbiddenException("You do not have permission to generate this certificate");

            if (enrollment.ProgressPercent < 100)
                throw new BadRequestException($"Course not completed ({enrollment.ProgressPercent:F2}%)");

            var existingCerts = await _unitOfWork.Certificates.FindAsync(c => c.EnrollmentId == enrollmentId);
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            var course = await _unitOfWork.Courses.GetByIdAsync(enrollment.CourseId);

            if (existingCerts.Any())
            {
                var cert = existingCerts.First();
                var dto = _mapper.Map<CertificateDto>(cert);
                dto.CourseName = course.Title;
                dto.HtmlContent = await PrepareHtmlContentAsync(user.FullName, course.Title, cert.IssuedAt, cert.Id.ToString());
                dto.AverageGrade = await CalculateAverageGradeAsync(enrollmentId);
                dto.StudentName = user.FullName;
                return dto;
            }

            return await GenerateNewCertificateInternalAsync(user, course, enrollment);
        }

        private async Task<CertificateDto> GenerateNewCertificateInternalAsync(
            Domain.Entities.Users.AppUser user,
            Domain.Entities.Courses.Course course,
            Domain.Entities.Enrollments.Enrollment enrollment)
        {
            string certGuid = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string pdfFileName = $"cert_{enrollment.Id}_{certGuid}.pdf";
            string pdfFolderPath = Path.Combine(_env.WebRootPath, "certificates");
            string pdfFilePath = Path.Combine(pdfFolderPath, pdfFileName);

            if (!Directory.Exists(pdfFolderPath)) Directory.CreateDirectory(pdfFolderPath);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu" }
            });

            await using var page = await browser.NewPageAsync();

            string htmlContent = await PrepareHtmlContentAsync(user.FullName, course.Title, DateTime.UtcNow, certGuid);

            await page.SetContentAsync(htmlContent, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
            await page.EmulateMediaTypeAsync(MediaType.Print);
            await page.PdfAsync(pdfFilePath, new PdfOptions { Format = PaperFormat.A4, PrintBackground = true });

            var newCert = new Certificate
            {
                EnrollmentId = enrollment.Id,
                OwnerFullName = user.FullName,
                IssuedAt = DateTime.UtcNow,
                CertificateUrl = $"/certificates/{pdfFileName}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Certificates.AddAsync(newCert);
            await _unitOfWork.SaveChangesAsync();

            var finalDto = _mapper.Map<CertificateDto>(newCert);
            finalDto.CourseName = course.Title;
            finalDto.HtmlContent = htmlContent;
            finalDto.AverageGrade = await CalculateAverageGradeAsync(enrollment.Id);
            finalDto.StudentName = user.FullName;

            return finalDto;
        }

        private async Task<string> PrepareHtmlContentAsync(string studentName, string courseName, DateTime date, string certId)
        {
            string templatePath = Path.Combine(_env.WebRootPath, "templates", "certificate.html");
            if (!File.Exists(templatePath)) throw new NotFoundException("Template not found");

            string html = await File.ReadAllTextAsync(templatePath);
            return html.Replace("{{StudentName}}", studentName)
                       .Replace("{{CourseName}}", courseName)
                       .Replace("{{Date}}", date.ToString("dd MMMM yyyy"))
                       .Replace("{{CertificateId}}", certId);
        }

        private async Task<double> CalculateAverageGradeAsync(int enrollmentId)
        {
            var attempts = await _unitOfWork.Attempts.FindAsync(a => a.EnrollmentId == enrollmentId && a.IsPassed);
            if (!attempts.Any()) return 0;

            var bestGrades = attempts.GroupBy(a => a.QuizId)
                                     .Select(g => g.Max(a => a.Percentage));

            return Math.Round(bestGrades.Average(), 2);
        }
    }
}