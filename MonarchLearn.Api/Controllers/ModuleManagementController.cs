using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Modules;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/management/courses/{courseId:int}/modules")]
    [Authorize(Roles = "Instructor,Admin")]
    public class ModuleManagementController : BaseController
    {
        private readonly IModuleService _moduleService;

        public ModuleManagementController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int courseId)
        {
            var modules = await _moduleService.GetModulesByCourseIdAsync(courseId);
            return Ok(modules);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int courseId, [FromBody] CreateModuleDto model)
        {
            if (courseId != model.CourseId)
                return BadRequest(new { message = "CourseId mismatch" });

            var moduleId = await _moduleService.CreateModuleAsync(CurrentUserId, model, IsAdmin);
            return Created(string.Empty, new { id = moduleId });
        }

        [HttpPut("~/api/v1/management/modules/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateModuleDto model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "ID mismatch" });

            await _moduleService.UpdateModuleAsync(CurrentUserId, model, IsAdmin);
            return Ok(new { message = "Module updated successfully" });
        }

        [HttpDelete("~/api/v1/management/modules/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _moduleService.DeleteModuleAsync(CurrentUserId, id, IsAdmin);
            return NoContent();
        }
    }
}