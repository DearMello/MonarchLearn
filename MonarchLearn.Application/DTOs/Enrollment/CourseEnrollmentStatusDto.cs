using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Enrollment
{
    public class CourseEnrollmentStatusDto
    {
        public bool IsEnrolled { get; set; }
        public int? EnrollmentId { get; set; }
        public double ProgressPercent { get; set; }
    }
}
