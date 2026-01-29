using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Admin
{
    public class TopCourseDto
    {
        public string CourseTitle { get; set; }
        public int EnrollmentCount { get; set; }
        public double PopularityScore { get; set; }
    }
}
