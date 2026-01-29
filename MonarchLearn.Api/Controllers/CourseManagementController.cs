using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.CourseAdmin;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/management/courses")]
    [Authorize(Roles = "Instructor,Admin")]
    public class CourseManagementController : BaseController
    {
        private readonly ICourseManagementService _courseManagementService;

        public CourseManagementController(ICourseManagementService courseManagementService)
        {
            _courseManagementService = courseManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseManagementService.GetInstructorCoursesAsync(CurrentUserId);
            return Ok(courses);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateCourseDto model)
        {
            var courseId = await _courseManagementService.CreateCourseAsync(CurrentUserId, model);
            return CreatedAtAction(nameof(GetAll), new { id = courseId }, new { id = courseId });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCourseDto model)
        {
            if (id != model.Id) return BadRequest(new { message = "ID mismatch" });

            await _courseManagementService.UpdateCourseAsync(CurrentUserId, model, IsAdmin);
            return Ok(new { message = "Course updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseManagementService.DeleteCourseAsync(CurrentUserId, id, IsAdmin);
            return NoContent();
        }
    }
}