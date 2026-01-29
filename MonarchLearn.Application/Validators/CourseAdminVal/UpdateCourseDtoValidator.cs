using FluentValidation;
using MonarchLearn.Application.DTOs.CourseAdmin;
using System.Linq;

namespace MonarchLearn.Application.Validators.CourseAdminVal
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid course ID");

            // Title: Yalnız null deyilsə yoxla
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title cannot be empty")
                .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters")
                .MinimumLength(5).WithMessage("Course title must be at least 5 characters")
                .When(x => x.Title != null);

            // ShortDescription: Yalnız null deyilsə yoxla
            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description cannot be empty")
                .MaximumLength(500).WithMessage("Short description cannot exceed 500 characters")
                .When(x => x.ShortDescription != null);

            // AboutCourse: Yalnız null deyilsə yoxla
            RuleFor(x => x.AboutCourse)
                .NotEmpty().WithMessage("Course description cannot be empty")
                .MinimumLength(50).WithMessage("Course description must be at least 50 characters")
                .When(x => x.AboutCourse != null);

            // ID-lər: Əgər 0-dan böyükdürsə (yəni göndərilibsə) yoxla
            // Qeyd: DTO-da bu ID-lər int? (nullable) olmalıdır ki, göndərilməyəndə null gəlsin.
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category must be selected")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.LevelId)
                .GreaterThan(0).WithMessage("Level must be selected")
                .When(x => x.LevelId.HasValue);

            RuleFor(x => x.LanguageId)
                .GreaterThan(0).WithMessage("Language must be selected")
                .When(x => x.LanguageId.HasValue);

            RuleFor(x => x.SkillIds)
               .Must(skills => skills == null || skills.Count <= 10)
               .WithMessage("Maximum 10 skills can be assigned to a course");

            RuleFor(x => x.CourseImage)
                .Must(file => file == null || IsValidImageExtension(file.FileName))
                .WithMessage("Only .jpg, .jpeg, .png, .webp files are allowed");
        }

        private bool IsValidImageExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}