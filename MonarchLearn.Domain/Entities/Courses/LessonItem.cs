using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Enums;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Courses
{
    public class LessonItem : SoftDeletableEntity
    {
       
        public int ModuleId { get; set; }
        public Module Module { get; set; }

        public string Title { get; set; }
        public int Order { get; set; }
        public LessonType LessonType { get; set; }

        public string? VideoUrl { get; set; }
        public string? ReadingText { get; set; }
        public int? EstimatedMinutes { get; set; }
        public int? VideoDurationSeconds { get; set; }
        public bool IsPreviewable { get; set; }
        public Quiz? Quiz { get; set; }
        public ICollection<LessonProgress> LessonProgresses { get; set; }
    }
}
