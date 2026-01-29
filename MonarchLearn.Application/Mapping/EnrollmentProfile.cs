using AutoMapper;
using MonarchLearn.Application.DTOs.Enrollment;
using MonarchLearn.Domain.Entities.Enrollments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class EnrollmentProfile : Profile
    {
        public EnrollmentProfile()
        {
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.CourseImageUrl, opt => opt.MapFrom(src => src.Course.CourseImageUrl))
                .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.Course.InstructorId))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Course.Instructor.FullName))
                // ✅ ƏLAVƏ ET:
                .ForMember(dest => dest.CertificateUrl,
                           opt => opt.MapFrom(src => src.Certificate != null ? src.Certificate.CertificateUrl : null));
        }
    }
}