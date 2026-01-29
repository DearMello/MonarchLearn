using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; }
        public string? ReviewerImage { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ParentReviewId { get; set; }
        public List<ReviewDto> Replies { get; set; } = new List<ReviewDto>();
    }
}
