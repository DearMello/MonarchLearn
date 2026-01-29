using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Courses
{
    public class CourseFilterDto
    {
        public string? SearchTerm { get; set; } 
        public int? CategoryId { get; set; }    
        public int? LevelId { get; set; }      
        public int? LanguageId { get; set; }   
        public double? MinRating { get; set; }  
        public List<int>? SkillIds { get; set; }
       
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
