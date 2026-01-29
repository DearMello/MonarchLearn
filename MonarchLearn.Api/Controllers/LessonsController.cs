using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/lessons")]
    [Authorize(Roles = "Student")]
    public class LessonsController : BaseController
    {
        private readonly ILessonCompletionService _lessonCompletionService;

        public LessonsController(ILessonCompletionService lessonCompletionService)
        {
            _lessonCompletionService = lessonCompletionService;
        }

        [HttpPost("{id:int}/completions")]
        public async Task<IActionResult> CreateCompletion(int id, [FromBody] CompleteLessonDto model)
        {
            if (id != model.LessonItemId)
                return BadRequest(new { message = "LessonId mismatch" });

            await _lessonCompletionService.CompleteLessonAsync(
                CurrentUserId,
                model.CourseId,
                id,
                model.WatchedSeconds ?? 0,
                model.MarkAsFinished
            );

            return Created(string.Empty, new { message = "Lesson marked as completed" });
        }

        [HttpGet("{id:int}/progress")]
        public async Task<IActionResult> GetProgress(int id, [FromQuery] int courseId)
        {
            var progress = await _lessonCompletionService.GetLessonProgressForStudentAsync(CurrentUserId, courseId, id);
            return Ok(progress);
        }
    }
}