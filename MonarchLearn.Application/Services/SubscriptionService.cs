using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Subscriptions;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Subscriptions;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SubscriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<SubscriptionPlanDto>> GetAllPlansAsync()
        {
            _logger.LogDebug("Fetching all active subscription plans");

            try
            {
                var plans = await _unitOfWork.SubscriptionPlans
                    .FindAsync(p => p.IsActive);

                var orderedPlans = plans
                    .OrderBy(p => p.Price)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} active subscription plan(s)", orderedPlans.Count);

                return _mapper.Map<List<SubscriptionPlanDto>>(orderedPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch subscription plans");
                throw new BadRequestException("Failed to load subscription plans. Please try again later.");
            }
        }

        public async Task<UserSubscriptionDto?> GetUserCurrentSubscriptionAsync(int userId)
        {
            _logger.LogDebug("Fetching current subscription for User {UserId}", userId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Subscription fetch failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Subscription fetch failed: User {UserId} is deleted", userId);
                throw new ForbiddenException("Your account has been deactivated");
            }

            var subscription = await _unitOfWork.UserSubscriptions.GetActiveSubscriptionAsync(userId);

            if (subscription == null)
            {
                _logger.LogInformation("No active subscription found for User {UserId}", userId);
                return null;
            }

            _logger.LogInformation(
                "Active subscription found for User {UserId}: Plan '{PlanName}', Expires {EndDate}",
                userId, subscription.SubscriptionPlan?.Name, subscription.EndDate.ToShortDateString());

            return _mapper.Map<UserSubscriptionDto>(subscription);
        }

        public async Task PurchaseSubscriptionAsync(int userId, int planId)
        {
            _logger.LogInformation(
                "Subscription purchase initiated: User {UserId}, Plan {PlanId}",
                userId, planId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Purchase failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Purchase failed: User {UserId} is deleted", userId);
                throw new ForbiddenException("Your account has been deactivated");
            }

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(planId);
            if (plan == null)
            {
                _logger.LogWarning("Purchase failed: Plan {PlanId} not found", planId);
                throw new NotFoundException("Subscription Plan", planId);
            }

            if (plan.IsDeleted || !plan.IsActive)
            {
                _logger.LogWarning(
                    "Purchase failed: Plan {PlanId} is unavailable (Deleted: {IsDeleted}, Active: {IsActive})",
                    planId, plan.IsDeleted, plan.IsActive);
                throw new BadRequestException("This subscription plan is no longer available");
            }

            if (plan.DurationDays <= 0)
            {
                _logger.LogError("Invalid plan configuration: Plan {PlanId} has DurationDays = {Duration}",
                    planId, plan.DurationDays);
                throw new BadRequestException("Invalid subscription plan configuration. Please contact support");
            }

            var currentSub = await _unitOfWork.UserSubscriptions.GetActiveSubscriptionAsync(userId);
            if (currentSub != null)
            {
                _logger.LogWarning(
                    "Purchase denied: User {UserId} already has active subscription (Plan: {CurrentPlanId}, Expires: {EndDate})",
                    userId, currentSub.SubscriptionPlanId, currentSub.EndDate);

                var currentPlanName = currentSub.SubscriptionPlan?.Name ?? "Unknown";
                throw new ConflictException(
                    $"You already have an active '{currentPlanName}' subscription. " +
                    $"It expires on {currentSub.EndDate.ToShortDateString()}.");
            }

            if (plan.Price == 0)
            {
                _logger.LogDebug("Processing free trial plan: {PlanName}", plan.Name);

                var pastFreeTrial = await _unitOfWork.UserSubscriptions
                    .FindAsync(s => s.UserId == userId && s.SubscriptionPlanId == planId);

                if (pastFreeTrial.Any())
                {
                    _logger.LogWarning(
                        "Free trial denied: User {UserId} already used Plan {PlanId}",
                        userId, planId);
                    throw new ConflictException(
                        "You have already used the free trial for this plan. Please choose a paid plan.");
                }

                var userEnrollments = await _unitOfWork.Enrollments.FindAsync(
                    e => e.UserId == userId && !e.IsDeleted);

                if (userEnrollments.Any())
                {
                    _logger.LogWarning(
                        "Free trial denied: User {UserId} already has {Count} enrollment(s)",
                        userId, userEnrollments.Count());
                    throw new ConflictException(
                        "Free trial allows enrollment in only 1 course. You already have active enrollments.");
                }

                _logger.LogInformation("Free trial approved for User {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Processing payment: {Amount} AZN for User {UserId}", plan.Price, userId);

                bool paymentSuccess = await SimulatePaymentAsync(userId, plan.Price);
                if (!paymentSuccess)
                {
                    _logger.LogError("Payment failed for User {UserId}, Amount: {Amount}", userId, plan.Price);
                    throw new BadRequestException(
                        "Payment processing failed. Please check your payment details and try again.");
                }

                _logger.LogInformation("Payment successful: {Amount} AZN from User {UserId}", plan.Price, userId);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddDays(plan.DurationDays);

                var newSub = new UserSubscription
                {
                    UserId = userId,
                    SubscriptionPlanId = planId,
                    StartDate = startDate,
                    EndDate = endDate,
                    CreatedAt = startDate,
                    IsDeleted = false
                };

                await _unitOfWork.UserSubscriptions.AddAsync(newSub);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Subscription created successfully: User {UserId}, Plan '{PlanName}', Duration {Days} days, Expires {EndDate}",
                    userId, plan.Name, plan.DurationDays, endDate.ToShortDateString());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create subscription for User {UserId}, Plan {PlanId}", userId, planId);
                throw new BadRequestException("Failed to activate subscription. Please contact support.");
            }
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto model)
        {
            _logger.LogInformation("Creating new subscription plan: {Name}", model.Name);

            var plan = new SubscriptionPlan
            {
                Name = model.Name,
                DurationDays = model.DurationDays,
                Price = model.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SubscriptionPlans.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Subscription plan created: ID {PlanId}", plan.Id);

            return _mapper.Map<SubscriptionPlanDto>(plan);
        }

        public async Task UpdatePlanAsync(UpdatePlanDto model)
        {
            _logger.LogInformation("Updating subscription plan: ID {PlanId}", model.Id);

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(model.Id);
            if (plan == null)
                throw new NotFoundException("Subscription Plan", model.Id);

            plan.Name = model.Name;
            plan.DurationDays = model.DurationDays;
            plan.Price = model.Price;
            plan.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Subscription plan updated: ID {PlanId}", model.Id);
        }

        public async Task DeletePlanAsync(int planId)
        {
            _logger.LogInformation("Deleting subscription plan: ID {PlanId}", planId);

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(planId);
            if (plan == null)
                throw new NotFoundException("Subscription Plan", planId);

            var activeSubscriptions = await _unitOfWork.UserSubscriptions.FindAsync(
                s => s.SubscriptionPlanId == planId && s.EndDate > DateTime.UtcNow);

            if (activeSubscriptions.Any())
            {
                _logger.LogWarning("Cannot delete plan {PlanId}: {Count} active subscriptions exist",
                    planId, activeSubscriptions.Count());
                throw new BadRequestException("Cannot delete plan with active subscriptions");
            }

            plan.IsDeleted = true;
            plan.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Subscription plan deleted: ID {PlanId}", planId);
        }

        private async Task<bool> SimulatePaymentAsync(int userId, decimal amount)
        {
            _logger.LogDebug("SIMULATED PAYMENT: Charging {Amount} AZN from User {UserId}", amount, userId);

            await Task.Delay(500);

            var random = new Random();
            bool success = random.Next(100) < 95;

            if (success)
            {
                _logger.LogInformation("SIMULATED PAYMENT SUCCESS: {Amount} AZN from User {UserId}", amount, userId);
            }
            else
            {
                _logger.LogWarning("SIMULATED PAYMENT FAILED: User {UserId}, Amount {Amount}", userId, amount);
            }

            return success;
        }
    }
}