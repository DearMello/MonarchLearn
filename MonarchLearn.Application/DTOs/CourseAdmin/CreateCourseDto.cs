using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.CourseAdmin
{
    public class CreateCourseDto
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string AboutCourse { get; set; }

        
        public IFormFile? CourseImage { get; set; }  

        public int CategoryId { get; set; }
        public int LevelId { get; set; }
        public int LanguageId { get; set; }
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}
