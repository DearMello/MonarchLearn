using FluentValidation;
using MonarchLearn.Application.DTOs.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.ModulesVal
{
    public class CreateModuleDtoValidator : AbstractValidator<CreateModuleDto>
    {
        public CreateModuleDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Invalid course ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Module title is required")
                .MaximumLength(200).WithMessage("Module title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Module title must be at least 3 characters");
        }
    }
}
