using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.LearningProgress
{
    public class Answer : BaseEntity
    {
        public int AttemptId { get; set; }
        public Attempt Attempt { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int SelectedOptionId { get; set; }
        public Option SelectedOption { get; set; }
    }
}
