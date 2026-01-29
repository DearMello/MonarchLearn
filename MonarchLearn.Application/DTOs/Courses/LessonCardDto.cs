using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Courses
{
    public class LessonCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } 
        public int DurationSeconds { get; set; }
        public bool IsPreviewable { get; set; } 
    }
}
