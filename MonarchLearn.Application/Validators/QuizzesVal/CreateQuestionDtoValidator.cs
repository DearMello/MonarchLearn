using FluentValidation;
using MonarchLearn.Application.DTOs.Quizzes;

namespace MonarchLearn.Application.Validators.QuizzesVal
{
    public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
    {
        public CreateQuestionDtoValidator()
        {
            // QuizId silindi, çünki URL-dən gəlir

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Question text is required")
                .MaximumLength(1000)
                .WithMessage("Question text cannot exceed 1000 characters");
        }
    }
}