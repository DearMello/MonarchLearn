using AutoMapper;
using MonarchLearn.Application.DTOs.Certificates;
using MonarchLearn.Domain.Entities.LearningProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class CertificateProfile : Profile
    {
        public CertificateProfile()
        {
            CreateMap<Certificate, CertificateDto>()
                .ForMember(dest => dest.CertificateId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.OwnerFullName))
                .ForMember(dest => dest.PdfUrl, opt => opt.MapFrom(src => src.CertificateUrl))
                .ForMember(dest => dest.AverageGrade, opt => opt.Ignore())
                // Bu sahələr bazada (Entity-də) yoxdur, onları servisdə əllə verəcəyik
                .ForMember(dest => dest.CourseName, opt => opt.Ignore())
                .ForMember(dest => dest.HtmlContent, opt => opt.Ignore());
        }
    }
}
