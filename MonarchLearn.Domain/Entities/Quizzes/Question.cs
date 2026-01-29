using Microsoft.VisualBasic.FileIO;
using MonarchLearn.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Quizzes
{
    public class Question : SoftDeletableEntity
    {
       
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public ICollection<Option> Options { get; set; } = new List<Option>();
    }

}
