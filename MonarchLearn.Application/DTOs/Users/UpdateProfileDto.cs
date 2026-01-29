using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Users
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? DesiredCareer { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
