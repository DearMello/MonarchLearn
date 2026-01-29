using MonarchLearn.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Quizzes
{
    public class Option : SoftDeletableEntity
    {
       
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

}
