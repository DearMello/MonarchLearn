using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class CompleteLessonDto
    {
        public int CourseId { get; set; }
        public int LessonItemId { get; set; }
        public int? WatchedSeconds { get; set; } 
        public bool MarkAsFinished { get; set; }
    }
}
