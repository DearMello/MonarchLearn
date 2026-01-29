using FluentValidation;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Domain.Entities.Enums;
using System;

namespace MonarchLearn.Application.Validators.LessonsVal
{
    public class UpdateLessonDtoValidator : AbstractValidator<UpdateLessonDto>
    {
        public UpdateLessonDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid lesson ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Lesson title is required")
                .MaximumLength(200).WithMessage("Lesson title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Lesson title must be at least 3 characters")
                .When(x => x.Title != null);

            RuleFor(x => x.LessonType)
                .IsInEnum().WithMessage("Invalid lesson type")
                .When(x => x.LessonType != null);

            RuleFor(x => x.VideoDurationSeconds)
               .GreaterThanOrEqualTo(0).WithMessage("Video duration cannot be negative")
               .LessThanOrEqualTo(86400).WithMessage("Video duration cannot exceed 24 hours")
               .When(x => x.LessonType == LessonType.Video && x.VideoDurationSeconds != null);

            RuleFor(x => x.ReadingText)
                .NotEmpty().WithMessage("Reading text is required for reading lessons")
                .MinimumLength(50).WithMessage("Reading text must be at least 50 characters")
                .When(x => x.LessonType == LessonType.Reading && x.ReadingText != null);

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("Order must be greater than 0")
                .When(x => x.Order != null);

            RuleFor(x => x.EstimatedMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated minutes cannot be negative")
                .LessThanOrEqualTo(1440).WithMessage("Estimated minutes cannot exceed 24 hours")
                .When(x => x.EstimatedMinutes != null);
        }
    }
}