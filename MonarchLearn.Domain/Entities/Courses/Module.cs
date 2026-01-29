using MonarchLearn.Domain.Entities.Common;

namespace MonarchLearn.Domain.Entities.Courses
{
    public class Module : SoftDeletableEntity
    {
       
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public ICollection<LessonItem> LessonItems { get; set; } = new List<LessonItem>();
    }

}
