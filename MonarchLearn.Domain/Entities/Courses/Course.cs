using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Enrollments;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Courses
{
    public class Course : SoftDeletableEntity
    {

        public int InstructorId { get; set; }
        public AppUser Instructor { get; set; }

        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string AboutCourse { get; set; }
        public string CourseImageUrl { get; set; }

        public int CategoryId { get; set; }
        public CourseCategory Category { get; set; }

        public int LevelId { get; set; }
        public CourseLevel Level { get; set; }

        public int LanguageId { get; set; }
        public CourseLanguage Language { get; set; }

        public int TotalDurationSeconds { get; set; }

        // Cached rating properties
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Navigation Properties
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
        public CourseStatistics? Statistics { get; set; }
    }



}
