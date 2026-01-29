using AutoMapper;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Domain.Entities.LearningProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class LearningProfile : Profile
    {
        public LearningProfile()
        {
            CreateMap<LessonProgress, LessonProgressDto>();
        }
    }
}
