using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Subscriptions;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/subscriptions")]
    [Authorize(Roles = "Student")]
    public class SubscriptionsController : BaseController
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrent()
        {
            var subscription = await _subscriptionService.GetUserCurrentSubscriptionAsync(CurrentUserId);

            if (subscription == null)
                return NotFound(new { message = "No active subscription found" });

            return Ok(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseSubscriptionDto model)
        {
            await _subscriptionService.PurchaseSubscriptionAsync(CurrentUserId, model.PlanId);
            return Created(string.Empty, new { message = "Subscription purchased successfully" });
        }
    }
}