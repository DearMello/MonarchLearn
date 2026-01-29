using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class LessonDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } // Enum-u string kimi qaytarmaq ucun (Front üçün rahat olsun)
        public int Order { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsPreviewable { get; set; }

        
        public string? VideoUrl { get; set; }
        public string? ReadingText { get; set; }
    }
}
