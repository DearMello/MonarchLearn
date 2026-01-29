using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Courses
{
    public class CourseModuleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<LessonCardDto> LessonItems { get; set; }
    }
}
