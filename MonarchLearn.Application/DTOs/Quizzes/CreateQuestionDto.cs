using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class CreateQuestionDto
    {
        
        public string Text { get; set; }

       
        public int Order { get; set; } = 0;
    }
}
