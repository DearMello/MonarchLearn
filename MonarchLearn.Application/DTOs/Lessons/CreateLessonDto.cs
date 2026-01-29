using Microsoft.AspNetCore.Http;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class CreateLessonDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public LessonType LessonType { get; set; }
        public string? VideoUrl { get; set; }
        public IFormFile? VideoFile { get; set; }
        public string? ReadingText { get; set; }
        

        public int? EstimatedMinutes { get; set; }
        public int Order { get; set; }
        public int? VideoDurationSeconds { get; set; }
        public bool IsPreviewable { get; set; } = false;
    }
}
