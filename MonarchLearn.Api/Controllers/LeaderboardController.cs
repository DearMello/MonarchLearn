using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/leaderboard")]
    [Authorize]
    public class LeaderboardController : BaseController
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int count = 10)
        {
            var leaderboard = await _leaderboardService.GetGlobalLeaderboardAsync(count);
            return Ok(leaderboard);
        }

        [HttpGet("~/api/v1/users/me/rank")]
        public async Task<IActionResult> GetMyRank()
        {
            var rank = await _leaderboardService.GetUserRankAsync(CurrentUserId);
            return Ok(rank);
        }
    }
}