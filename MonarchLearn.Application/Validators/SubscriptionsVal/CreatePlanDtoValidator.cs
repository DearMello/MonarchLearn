using FluentValidation;
using MonarchLearn.Application.DTOs.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.SubscriptionsVal
{
    public class CreatePlanDtoValidator : AbstractValidator<CreatePlanDto>
    {
        public CreatePlanDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Plan name is required")
                .MaximumLength(100).WithMessage("Plan name cannot exceed 100 characters")
                .MinimumLength(3).WithMessage("Plan name must be at least 3 characters");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0).WithMessage("Duration must be at least 1 day")
                .LessThanOrEqualTo(3650).WithMessage("Duration cannot exceed 10 years (3650 days)");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative")
                .LessThanOrEqualTo(230).WithMessage("Price cannot exceed $230");
        }
    }
}
