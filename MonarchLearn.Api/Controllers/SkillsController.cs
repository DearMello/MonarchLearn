using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/skills")]
    public class SkillsController : BaseController
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var skills = await _skillService.GetAllSkillsAsync();
            return Ok(skills);
        }

        [HttpPost("~/api/v1/management/skills")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLookupDto model)
        {
            var skill = await _skillService.CreateSkillAsync(model);
            return Created(string.Empty, skill);
        }

        [HttpPut("~/api/v1/management/skills/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLookupDto model)
        {
            await _skillService.UpdateSkillAsync(id, model);
            return Ok(new { message = "Skill updated successfully" });
        }

        [HttpDelete("~/api/v1/management/skills/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _skillService.DeleteSkillAsync(id);
            return NoContent();
        }
    }
}