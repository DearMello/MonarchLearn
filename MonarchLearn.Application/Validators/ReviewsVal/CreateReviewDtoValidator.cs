using FluentValidation;
using MonarchLearn.Application.DTOs.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.ReviewsVal
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Invalid course ID");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Review comment is required")
                .MaximumLength(1000).WithMessage("Review cannot exceed 1000 characters");

            RuleFor(x => x.ParentReviewId)
                .GreaterThan(0).WithMessage("Invalid parent review ID")
                .When(x => x.ParentReviewId.HasValue);
        }
    }
}
