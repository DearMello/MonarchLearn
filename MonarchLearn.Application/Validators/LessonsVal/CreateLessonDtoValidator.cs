using FluentValidation;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Domain.Entities.Enums;
using System;

namespace MonarchLearn.Application.Validators.LessonsVal
{
    public class CreateLessonDtoValidator : AbstractValidator<CreateLessonDto>
    {
        public CreateLessonDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0).WithMessage("Invalid module selection.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Lesson title is required.")
                .MinimumLength(3).WithMessage("Lesson title must be at least 3 characters.")
                .MaximumLength(200).WithMessage("Lesson title cannot exceed 200 characters.");

            RuleFor(x => x.LessonType)
                .IsInEnum().WithMessage("Please select a valid lesson type (Video, Reading, or Quiz).");

            // --- VIDEO LOGIC ---
            RuleFor(x => x)
                .Must(x => x.VideoFile != null || !string.IsNullOrEmpty(x.VideoUrl))
                .WithMessage("For video lessons, you must either upload a video file or provide a video URL.")
                .When(x => x.LessonType == LessonType.Video);

            RuleFor(x => x.VideoDurationSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Video duration cannot be negative.")
                .LessThanOrEqualTo(86400).WithMessage("Lesson duration cannot exceed 24 hours.")
                .When(x => x.LessonType == LessonType.Video);

            // --- READING LOGIC ---
            RuleFor(x => x.ReadingText)
                .NotEmpty().WithMessage("Reading text is required for reading lessons.")
                .MinimumLength(50).WithMessage("Reading text is too short (minimum 50 characters).")
                .When(x => x.LessonType == LessonType.Reading);

            // --- GENERAL FIELDS ---
            RuleFor(x => x.EstimatedMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated minutes cannot be negative.")
                .LessThanOrEqualTo(1440).WithMessage("Estimated time cannot exceed 24 hours.");

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("Lesson order must be greater than 0.");
        }
    }
}