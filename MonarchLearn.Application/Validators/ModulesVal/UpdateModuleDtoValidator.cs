using FluentValidation;
using MonarchLearn.Application.DTOs.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.ModulesVal
{
    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid module ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Module title is required")
                .MaximumLength(200).WithMessage("Module title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Module title must be at least 3 characters")
                .When(x => x.Title != null);

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("Order must be greater than 0")
                .When(x => x.Order != null);
        }
    }
}