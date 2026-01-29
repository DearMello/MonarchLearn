using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizResultDto
    {
        public int AttemptId { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public string Message { get; set; } // "Təbriklər" və ya "2 saat gözlə"
        public DateTime? NextAttemptAt { get; set; } // Növbəti cəhd vaxtı
    }
}
