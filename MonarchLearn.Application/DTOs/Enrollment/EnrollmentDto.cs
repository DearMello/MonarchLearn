using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } 
        public string CourseImageUrl { get; set; } 
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } 

        public DateTime StartedAt { get; set; }
        public double ProgressPercent { get; set; }
        public bool IsCompleted { get; set; }

        // Resume düyməsi üçün 
        public int? LastLessonItemId { get; set; }
        public string? CertificateUrl { get; set; }

        public double? AverageGrade { get; set; }


    }
}
