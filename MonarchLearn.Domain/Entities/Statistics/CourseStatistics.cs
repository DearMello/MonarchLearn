using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Statistics
{
    public class CourseStatistics : BaseEntity
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int ViewCount { get; set; }
        public double PopularityScore { get; set; }
    }

}
