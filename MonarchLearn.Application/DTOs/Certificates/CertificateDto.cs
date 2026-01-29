using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Certificates
{
    public class CertificateDto
    {
        public string CertificateId { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public DateTime IssuedAt { get; set; }

        // Ekranda göstərmək üçün (Preview HTML)
        public string HtmlContent { get; set; }
        public double AverageGrade { get; set; }

        // Yükləmək üçün (Download PDF Link)
        public string PdfUrl { get; set; }
    }
}
