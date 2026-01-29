using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Enrollments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.LearningProgress
{
    public class LessonProgress : BaseEntity
    {
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        public int LessonItemId { get; set; }
        public LessonItem LessonItem { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? WatchedSeconds { get; set; }
    }

}
