using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/categories")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        [HttpPost("~/api/v1/management/categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLookupDto model)
        {
            var result = await _categoryService.CreateCategoryAsync(model);
            return Created(string.Empty, result);
        }

        [HttpPut("~/api/v1/management/categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateLookupDto model)
        {
            await _categoryService.UpdateCategoryAsync(id, model);
            return Ok(new { message = "Category updated" });
        }

        [HttpDelete("~/api/v1/management/categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}