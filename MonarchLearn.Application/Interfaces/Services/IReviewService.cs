using MonarchLearn.Application.DTOs.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task AddReviewAsync(int userId, CreateReviewDto model);
        Task<(List<ReviewDto> Reviews, int Total)> GetCourseReviewsAsync(int courseId, int pageNumber = 1, int pageSize = 10);
        Task UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto model);
        Task DeleteReviewAsync(int userId, int reviewId);
    }
}
