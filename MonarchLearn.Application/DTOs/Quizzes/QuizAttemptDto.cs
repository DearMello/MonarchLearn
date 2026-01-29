using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizAttemptDto
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public double Percentage { get; set; }
        public bool IsPassed { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
