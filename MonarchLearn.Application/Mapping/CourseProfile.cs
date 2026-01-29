using AutoMapper;
using MonarchLearn.Application.DTOs.CourseAdmin;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Domain.Entities.Courses;
using System.Linq;

namespace MonarchLearn.Application.Mapping
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            // Lesson Mapping
            CreateMap<LessonItem, LessonCardDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.LessonType.ToString()))
                .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => (src.EstimatedMinutes ?? 0) * 60));

            // Module Mapping
            CreateMap<Module, CourseModuleDto>();

            // Course Card Mapping
            CreateMap<Course, CourseCardDto>()
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.CourseImageUrl))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src =>
                    (src.Reviews != null && src.Reviews.Any()) ? src.Reviews.Average(r => (double)r.Rating) : 0));

            // Course Detail Mapping
            CreateMap<Course, CourseDetailDto>()
                .IncludeBase<Course, CourseCardDto>()
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.AboutCourse))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                    src.Skills.Select(s => s.Name).ToList()));


            CreateMap<CreateCourseDto, Course>();

            CreateMap<UpdateCourseDto, Course>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
    {

        if (srcMember == null) return false;
        if (srcMember is int intVal && intVal == 0) return false;


        if (srcMember is System.Collections.IEnumerable list && !list.Cast<object>().Any()) return false;

        return true;
    }));
        }
    }
}