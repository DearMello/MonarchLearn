using FluentValidation;
using MonarchLearn.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.UsersVal
{
    public class UserEducationDtoValidator : AbstractValidator<UserEducationDto>
    {
        public UserEducationDtoValidator()
        {
            RuleFor(x => x.SchoolName)
                .NotEmpty().WithMessage("School name is required")
                .MaximumLength(200).WithMessage("School name cannot exceed 200 characters");

            RuleFor(x => x.Degree)
                .NotEmpty().WithMessage("Degree is required")
                .MaximumLength(100).WithMessage("Degree cannot exceed 100 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Start date cannot be in the future");

            RuleFor(x => x.GraduationDate)
                .GreaterThan(x => x.StartDate).WithMessage("Graduation date must be after start date")
                .When(x => x.GraduationDate.HasValue);
        }
    }
}
