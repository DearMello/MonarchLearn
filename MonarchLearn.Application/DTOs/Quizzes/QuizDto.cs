using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizDto
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public int PassingScorePercent { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }

        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}
