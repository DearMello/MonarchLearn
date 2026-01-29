using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/management")]
    [Authorize(Roles = "Instructor,Admin")]
    public class LessonManagementController : BaseController
    {
        private readonly ILessonService _lessonService;

        public LessonManagementController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost("modules/{moduleId:int}/lessons")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(int moduleId, [FromForm] CreateLessonDto model)
        {
            if (moduleId != model.ModuleId)
                return BadRequest(new { message = "ModuleId mismatch" });

            var result = await _lessonService.CreateLessonAsync(CurrentUserId, model, IsAdmin);
            return Created(string.Empty, result);
        }

        [HttpPut("lessons/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateLessonDto model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "ID mismatch" });

            await _lessonService.UpdateLessonAsync(CurrentUserId, model, IsAdmin);
            return Ok(new { message = "Lesson updated successfully" });
        }

        [HttpDelete("lessons/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _lessonService.DeleteLessonAsync(CurrentUserId, id, IsAdmin);
            return NoContent();
        }
    }
}