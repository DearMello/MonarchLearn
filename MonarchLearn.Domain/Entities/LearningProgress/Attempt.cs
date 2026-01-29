using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Enrollments;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.LearningProgress
{
    public class Attempt : BaseEntity
    {
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int Score { get; set; }
        public double Percentage { get; set; }
        public bool IsPassed { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
