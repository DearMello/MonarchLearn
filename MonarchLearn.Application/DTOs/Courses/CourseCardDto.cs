using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Courses
{
    public class CourseCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public string Level { get; set; } 
        public double Rating { get; set; }
        public string InstructorName { get; set; } 
        public int TotalDurationSeconds { get; set; }

        
        public int ViewCount { get; set; }
    }
}
