using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class CreateOptionDto
    {
        
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

        
        public int Order { get; set; } = 0;
    }
}
