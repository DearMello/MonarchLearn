using FluentValidation;
using MonarchLearn.Application.DTOs.CourseAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.CourseAdminVal
{
    public class CreateCourseDtoValidator : AbstractValidator<CreateCourseDto>
    {
        public CreateCourseDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required")
                .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters")
                .MinimumLength(5).WithMessage("Course title must be at least 5 characters");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required")
                .MaximumLength(500).WithMessage("Short description cannot exceed 500 characters")
                .MinimumLength(20).WithMessage("Short description must be at least 20 characters");

            RuleFor(x => x.AboutCourse)
                .NotEmpty().WithMessage("Course description is required")
                .MinimumLength(50).WithMessage("Course description must be at least 50 characters");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category must be selected");

            RuleFor(x => x.LevelId)
                .GreaterThan(0).WithMessage("Level must be selected");

            RuleFor(x => x.LanguageId)
                .GreaterThan(0).WithMessage("Language must be selected");

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
