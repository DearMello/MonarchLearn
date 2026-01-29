using FluentValidation;
using MonarchLearn.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.UsersVal
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[\p{L} \s'-]+$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.DesiredCareer)
                .MaximumLength(100).WithMessage("Desired career cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.DesiredCareer));

            RuleFor(x => x.ProfileImage)
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
