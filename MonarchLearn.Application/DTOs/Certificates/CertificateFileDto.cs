using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Certificates
{
    public class CertificateFileDto
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
    }
}
