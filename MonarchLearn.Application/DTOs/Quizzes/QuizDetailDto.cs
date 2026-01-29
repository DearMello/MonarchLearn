using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int PassingScorePercent { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public List<QuestionDetailDto> Questions { get; set; } = new List<QuestionDetailDto>();
    }
}
