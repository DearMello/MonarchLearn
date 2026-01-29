using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/levels")]
    public class LevelsController : BaseController
    {
        private readonly ILevelService _levelService;

        public LevelsController(ILevelService levelService)
        {
            _levelService = levelService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var levels = await _levelService.GetAllLevelsAsync();
            return Ok(levels);
        }

        [HttpPost("~/api/v1/management/levels")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLookupDto model)
        {
            var level = await _levelService.CreateLevelAsync(model);
            return Created(string.Empty, level);
        }

        [HttpPut("~/api/v1/management/levels/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLookupDto model)
        {
            await _levelService.UpdateLevelAsync(id, model);
            return Ok(new { message = "Level updated successfully" });
        }

        [HttpDelete("~/api/v1/management/levels/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _levelService.DeleteLevelAsync(id);
            return NoContent();
        }
    }
}