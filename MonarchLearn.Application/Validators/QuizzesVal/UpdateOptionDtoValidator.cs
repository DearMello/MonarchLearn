using FluentValidation;
using MonarchLearn.Application.DTOs.Quizzes;

namespace MonarchLearn.Application.Validators.QuizzesVal
{
    public class UpdateOptionDtoValidator : AbstractValidator<UpdateOptionDto>
    {
        public UpdateOptionDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Option ID is required");

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Option text is required")
                .MaximumLength(500)
                .WithMessage("Option text cannot exceed 500 characters");
        }
    }
}