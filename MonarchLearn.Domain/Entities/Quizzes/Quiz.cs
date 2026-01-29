using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.LearningProgress;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Quizzes
{
    public class Quiz : SoftDeletableEntity
    {
        
        public int LessonItemId { get; set; }
        public LessonItem LessonItem { get; set; }
        public string Title { get; set; }
        public int TimeLimitSeconds { get; set; }
        public int PassingScorePercent { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    }

}
