using FluentValidation;
using MonarchLearn.Application.DTOs.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Validators.CourseVal
{
    public class CourseFilterDtoValidator : AbstractValidator<CourseFilterDto>
    {
        public CourseFilterDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");
        }
    }
}
