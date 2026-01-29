using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class CreateQuizDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int PassingScorePercent { get; set; } = 50;
        public int? TimeLimitSeconds { get; set; }
    }
}
