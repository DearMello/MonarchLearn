using Microsoft.AspNetCore.Identity;
using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Enrollments;
using MonarchLearn.Domain.Entities.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Users
{
    public class AppUser : IdentityUser<int>, ISoftDeletable
    {
        public string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? DesiredCareer { get; set; }

        public bool IsDeleted { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserEducation> Educations { get; set; } = new List<UserEducation>();
        public ICollection<UserWorkExperience> WorkExperiences { get; set; } = new List<UserWorkExperience>();
        public ICollection<UserStreak> Streaks { get; set; } = new List<UserStreak>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
    }
}
