using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Users;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/users/me")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var profile = await _userService.GetUserProfileAsync(CurrentUserId);
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateProfileDto model)
        {
            await _userService.UpdateProfileAsync(CurrentUserId, model);
            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpPost("educations")]
        public async Task<IActionResult> CreateEducation([FromBody] UserEducationDto model)
        {
            var id = await _userService.AddEducationAsync(CurrentUserId, model);
            return Created(string.Empty, new { id, message = "Education added successfully" });
        }

        [HttpPut("educations/{id:int}")]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] UserEducationDto model)
        {
            await _userService.UpdateEducationAsync(CurrentUserId, id, model);
            return Ok(new { message = "Education updated successfully" });
        }

        [HttpDelete("educations/{id:int}")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            await _userService.DeleteEducationAsync(CurrentUserId, id);
            return NoContent();
        }

        [HttpPost("work-experiences")]
        public async Task<IActionResult> CreateWorkExperience([FromBody] UserWorkExperienceDto model)
        {
            var id = await _userService.AddWorkExperienceAsync(CurrentUserId, model);
            return Created(string.Empty, new { id, message = "Work experience added successfully" });
        }

        [HttpPut("work-experiences/{id:int}")]
        public async Task<IActionResult> UpdateWorkExperience(int id, [FromBody] UserWorkExperienceDto model)
        {
            await _userService.UpdateWorkExperienceAsync(CurrentUserId, id, model);
            return Ok(new { message = "Work experience updated successfully" });
        }

        [HttpDelete("work-experiences/{id:int}")]
        public async Task<IActionResult> DeleteWorkExperience(int id)
        {
            await _userService.DeleteWorkExperienceAsync(CurrentUserId, id);
            return NoContent();
        }
    }
}