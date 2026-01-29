using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        public int CourseId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; }

       
        // Əgər bu sadə rəydirsə null olacaq.
        // Əgər kiməsə cavabdırsa, həmin rəyin ID-si olacaq.
        public int? ParentReviewId { get; set; }
    }
}
