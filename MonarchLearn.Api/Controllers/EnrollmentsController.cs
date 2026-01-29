using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Enrollment;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/enrollments")]
    [Authorize(Roles = "Student,Admin,Instructor")]
    public class EnrollmentsController : BaseController
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnrollRequestDto model)
        {
            var enrollment = await _enrollmentService.EnrollStudentAsync(CurrentUserId, model.CourseId);
            return Created(string.Empty, enrollment);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(CurrentUserId);
            return Ok(enrollments);
        }

        [HttpGet("{courseId:int}/progress")]
        public async Task<IActionResult> GetProgress(int courseId)
        {
            var courseProgress = await _enrollmentService.GetCourseProgressAsync(CurrentUserId, courseId);
            return Ok(courseProgress);
        }
    }
}