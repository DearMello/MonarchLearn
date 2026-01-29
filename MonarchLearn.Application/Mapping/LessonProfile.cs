using AutoMapper;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Domain.Entities.Courses;

namespace MonarchLearn.Application.Mapping
{
    public class LessonProfile : Profile
    {
        public LessonProfile()
        {
            CreateMap<CreateLessonDto, LessonItem>();

            CreateMap<UpdateLessonDto, LessonItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<LessonItem, LessonDetailDto>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.EstimatedMinutes))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.LessonType.ToString()));
        }
    }
}