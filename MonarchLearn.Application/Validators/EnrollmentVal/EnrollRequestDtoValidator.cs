using FluentValidation;
using MonarchLearn.Application.DTOs.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.EnrollmentVal
{
    public class EnrollRequestDtoValidator : AbstractValidator<EnrollRequestDto>
    {
        public EnrollRequestDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Invalid course ID");
        }
    }
}
