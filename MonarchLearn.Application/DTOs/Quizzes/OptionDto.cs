using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class OptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
        // IsCorrect - Students must not see correct answers before submission!
    }
}
