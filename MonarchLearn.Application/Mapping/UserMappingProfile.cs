using AutoMapper;
using MonarchLearn.Application.DTOs.Users;
using MonarchLearn.Domain.Entities.Users;

namespace MonarchLearn.Application.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserEducation, UserEducationDto>();
            CreateMap<UserEducationDto, UserEducation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserWorkExperience, UserWorkExperienceDto>();
            CreateMap<UserWorkExperienceDto, UserWorkExperience>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AppUser, UserProfileDto>()
                .ForMember(dest => dest.CurrentStreak, opt => opt.Ignore());

            CreateMap<UpdateProfileDto, AppUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}