using FluentValidation;
using MonarchLearn.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.LookUpVal
{
    public class CreateLookupDtoValidator : AbstractValidator<CreateLookupDto>
    {
        public CreateLookupDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters");
        }
    }
}
