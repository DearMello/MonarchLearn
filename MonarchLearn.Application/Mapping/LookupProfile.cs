using AutoMapper;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Domain.Entities.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class LookupProfile : Profile
    {
        public LookupProfile()
        {
            // Entity -> DTO
            CreateMap<CourseCategory, LookupDto>();
            CreateMap<CourseLanguage, LookupDto>();
            CreateMap<CourseLevel, LookupDto>();
            CreateMap<Skill, LookupDto>();

            // DTO -> Entity (Yaratmaq üçün)
            CreateMap<CreateLookupDto, CourseCategory>();
            CreateMap<CreateLookupDto, CourseLanguage>();
            CreateMap<CreateLookupDto, CourseLevel>();
            CreateMap<CreateLookupDto, Skill>();
        }
    }
}
