using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizStatisticsDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int TotalAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public double PassRate { get; set; }
        public double AverageScore { get; set; }
    }
}
