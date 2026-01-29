using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Admin
{
    public class AbandonedCourseDto
    {
        public string CourseTitle { get; set; }
        public int EnrolledCount { get; set; }
        public int CompletedCount { get; set; }
        public double AbandonmentRate { get; set; } // %
    }
}
