using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Users
{
    public class UserEducationDto
    {
        public string SchoolName { get; set; }
        public string Degree { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? GraduationDate { get; set; }
    }
}
