using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Courses
{
    //to do : Baxarsan istesen Commenti elave entityede cixarda bilersen eger istesennnnnnnnn
    public class CourseReview : SoftDeletableEntity
    {
       
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        [ForeignKey("ParentReviewId")]
        public int? ParentReviewId { get; set; }
        public CourseReview? ParentReview { get; set; }
    }

}
