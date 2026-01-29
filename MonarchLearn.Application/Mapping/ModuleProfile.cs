using AutoMapper;
using MonarchLearn.Application.DTOs.Modules;
using MonarchLearn.Domain.Entities.Courses;

namespace MonarchLearn.Application.Mapping
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            CreateMap<CreateModuleDto, Module>();

            CreateMap<UpdateModuleDto, Module>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Module, ModuleWithLessonsDto>()
                .ForMember(dest => dest.Lessons, opt => opt.Ignore());
        }
    }
}