using Microsoft.AspNetCore.Http;
using MonarchLearn.Domain.Entities.Enums;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class UpdateLessonDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public LessonType? LessonType { get; set; }
        public string? VideoUrl { get; set; }
        public IFormFile? VideoFile { get; set; }
        public string? ReadingText { get; set; }
        public int? EstimatedMinutes { get; set; }
        public int? VideoDurationSeconds { get; set; }
        public int? Order { get; set; }
        public bool? IsPreviewable { get; set; }
    }
}