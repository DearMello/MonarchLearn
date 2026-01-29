using FluentValidation;
using MonarchLearn.Application.DTOs.Quizzes;

namespace MonarchLearn.Application.Validators.QuizzesVal
{
    public class CreateOptionDtoValidator : AbstractValidator<CreateOptionDto>
    {
        public CreateOptionDtoValidator()
        {
            // QuestionId silindi, çünki URL-dən gəlir

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Option text is required")
                .MaximumLength(500)
                .WithMessage("Option text cannot exceed 500 characters");
        }
    }
}