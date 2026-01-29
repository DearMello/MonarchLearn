using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Users
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? DesiredCareer { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CompletedCourseDto>? CompletedCourses { get; set; }
        public List<UserEducationDto> Educations { get; set; }
        public List<UserWorkExperienceDto> WorkExperiences { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? CurrentStreak { get; set; } 
    }

    public class CompletedCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime CompletedAt { get; set; }
        public double AverageGrade { get; set; }
    }
}
