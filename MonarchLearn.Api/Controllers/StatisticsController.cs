using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/statistics")]
    [AllowAnonymous]
    public class StatisticsController : BaseController
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("popular-courses")]
        public async Task<IActionResult> GetPopularCourses([FromQuery] int count = 10)
        {
            var result = await _statisticsService.GetMostPopularCoursesAsync(count);
            return Ok(result);
        }

        [HttpGet("trending-courses")]
        public async Task<IActionResult> GetTrendingCourses([FromQuery] int count = 10)
        {
            var result = await _statisticsService.GetTrendingCoursesAsync(count);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("total-views")]
        public async Task<IActionResult> GetTotalViews()
        {
            var result = await _statisticsService.GetTotalViewsAsync();
            return Ok(new { totalViews = result });
        }
    }
}