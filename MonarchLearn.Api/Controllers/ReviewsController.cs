using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Reviews;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/courses/{courseId:int}/reviews")]
    public class ReviewsController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            int courseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (reviews, total) = await _reviewService.GetCourseReviewsAsync(courseId, pageNumber, pageSize);

            return Ok(new
            {
                courseId,
                total,
                pageNumber,
                pageSize,
                reviews
            });
        }

        [HttpPost]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<IActionResult> Create(int courseId, [FromBody] CreateReviewDto model)
        {
            if (courseId != model.CourseId)
                return BadRequest(new { message = "CourseId mismatch" });

            await _reviewService.AddReviewAsync(CurrentUserId, model);
            return Created(string.Empty, new { message = "Review added successfully" });
        }

        [HttpPut("~/api/v1/reviews/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto model)
        {
            await _reviewService.UpdateReviewAsync(CurrentUserId, id, model);
            return Ok(new { message = "Review updated successfully" });
        }

        [HttpDelete("~/api/v1/reviews/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _reviewService.DeleteReviewAsync(CurrentUserId, id);
            return NoContent();
        }
    }
}