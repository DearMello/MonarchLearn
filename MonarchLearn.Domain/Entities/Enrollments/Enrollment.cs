using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Enrollments
{
    public class Enrollment : SoftDeletableEntity
    {
       
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public bool IsCompleted { get; set; }
        public double ProgressPercent { get; set; }
        public DateTime StartedAt { get; set; }
        public int? LastLessonItemId { get; set; }
        public LessonItem? LastLessonItem { get; set; }
        public int? CertificateId { get; set; }
        public Certificate? Certificate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
        public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    }

}
