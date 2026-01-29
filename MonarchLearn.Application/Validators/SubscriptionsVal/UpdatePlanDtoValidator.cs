using FluentValidation;
using MonarchLearn.Application.DTOs.Subscriptions;

namespace MonarchLearn.Application.Validators.Subscriptions
{
    public class UpdatePlanDtoValidator : AbstractValidator<UpdatePlanDto>
    {
        public UpdatePlanDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Plan ID is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Plan name is required")
                .MaximumLength(100)
                .WithMessage("Plan name cannot exceed 100 characters");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0)
                .WithMessage("Duration must be at least 1 day")
                .LessThanOrEqualTo(365)
                .WithMessage("Duration cannot exceed 365 days");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative")
                .LessThanOrEqualTo(10000)
                .WithMessage("Price cannot exceed 10000");
        }
    }
}