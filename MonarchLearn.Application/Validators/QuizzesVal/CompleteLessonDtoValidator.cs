using FluentValidation;
using MonarchLearn.Application.DTOs.Lessons;

namespace MonarchLearn.Application.Validators.LessonsVal
{
    public class CompleteLessonDtoValidator : AbstractValidator<CompleteLessonDto>
    {
        public CompleteLessonDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("Course ID is required");

            RuleFor(x => x.LessonItemId)
                .GreaterThan(0)
                .WithMessage("Lesson ID is required");

            RuleFor(x => x.WatchedSeconds)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Watched seconds cannot be negative")
                .LessThanOrEqualTo(86400)
                .WithMessage("Watched seconds cannot exceed 24 hours");
        }
    }
}