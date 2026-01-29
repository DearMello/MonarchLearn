using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Subscriptions;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/subscription-plans")]
    public class SubscriptionPlansController : BaseController
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionPlansController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _subscriptionService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpPost("~/api/v1/management/subscription-plans")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePlanDto model)
        {
            var plan = await _subscriptionService.CreatePlanAsync(model);
            return CreatedAtAction(nameof(GetAll), new { id = plan.Id }, plan);
        }

        [HttpPut("~/api/v1/management/subscription-plans/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanDto model)
        {
            if (id != model.Id) return BadRequest(new { message = "ID mismatch" });

            await _subscriptionService.UpdatePlanAsync(model);
            return Ok(new { message = "Plan updated" });
        }

        [HttpDelete("~/api/v1/management/subscription-plans/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _subscriptionService.DeletePlanAsync(id);
            return NoContent();
        }
    }
}