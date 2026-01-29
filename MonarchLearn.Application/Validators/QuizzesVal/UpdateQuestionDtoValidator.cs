using FluentValidation;
using MonarchLearn.Application.DTOs.Quizzes;

namespace MonarchLearn.Application.Validators.QuizzesVal
{
    public class UpdateQuestionDtoValidator : AbstractValidator<UpdateQuestionDto>
    {
        public UpdateQuestionDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Question ID is required");

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Question text is required")
                .MaximumLength(1000)
                .WithMessage("Question text cannot exceed 1000 characters");
        }
    }
}