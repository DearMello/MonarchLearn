using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/courses")]
    [AllowAnonymous]
    public class CoursesController : BaseController
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter)
        {
            var courses = await _courseService.GetCoursesAsync(filter);
            return Ok(courses);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetCourseDetailAsync(id);
            return Ok(course);
        }
    }
}