using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Courses
{
    public class CourseDetailDto : CourseCardDto
    {
        public string Description { get; set; } 
        public DateTime LastUpdated { get; set; }
        public string Language { get; set; }
        public List<string> Skills { get; set; } = new List<string>();

        public List<CourseModuleDto> Modules { get; set; } 
    }
}
