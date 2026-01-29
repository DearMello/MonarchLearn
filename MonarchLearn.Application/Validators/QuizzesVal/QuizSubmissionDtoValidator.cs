using FluentValidation;
using MonarchLearn.Application.DTOs.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.QuizzesVal
{
    public class QuizSubmissionDtoValidator : AbstractValidator<QuizSubmissionDto>
    {
        public QuizSubmissionDtoValidator()
        {
            RuleFor(x => x.QuizId)
                .GreaterThan(0).WithMessage("Invalid quiz ID");

            RuleFor(x => x.EnrollmentId)
                .GreaterThan(0).WithMessage("Invalid enrollment ID");

           
            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("At least one answer is required");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .GreaterThan(0).WithMessage("Invalid question ID");

                answer.RuleFor(a => a.SelectedOptionId)
                    .GreaterThan(0).WithMessage("Invalid option ID");
            });
        }
    }
}
