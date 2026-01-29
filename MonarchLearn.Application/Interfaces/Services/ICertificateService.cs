using MonarchLearn.Application.DTOs.Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ICertificateService
    {
        Task<CertificateDto> GenerateCertificateAsync(int userId, int enrollmentId);
        Task<CertificateDto> GetCertificateByIdAsync(int userId, int certificateId);
        Task<CertificateFileDto> GetCertificatePdfAsync(int userId, int certificateId);
    }
}
