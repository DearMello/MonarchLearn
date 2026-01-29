using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/languages")]
    public class LanguagesController : BaseController
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            return Ok(languages);
        }

        [HttpPost("~/api/v1/management/languages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLookupDto model)
        {
            var language = await _languageService.CreateLanguageAsync(model);
            return Created(string.Empty, language);
        }

        [HttpPut("~/api/v1/management/languages/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLookupDto model)
        {
            await _languageService.UpdateLanguageAsync(id, model);
            return Ok(new { message = "Language updated successfully" });
        }

        [HttpDelete("~/api/v1/management/languages/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _languageService.DeleteLanguageAsync(id);
            return NoContent();
        }
    }
}