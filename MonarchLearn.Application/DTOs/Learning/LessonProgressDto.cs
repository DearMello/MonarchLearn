using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class LessonProgressDto
    {
        public int LessonItemId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? WatchedSeconds { get; set; }
    }
}
