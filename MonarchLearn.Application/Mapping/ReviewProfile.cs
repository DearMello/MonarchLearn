using AutoMapper;
using MonarchLearn.Application.DTOs.Reviews;
using MonarchLearn.Domain.Entities.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            // CreateReviewDto -> CourseReview (ParentReviewId avtomatik oturur)
            CreateMap<CreateReviewDto, CourseReview>();

            // CourseReview -> ReviewDto
            CreateMap<CourseReview, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.ReviewerImage, opt => opt.MapFrom(src => src.User.ProfileImageUrl))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt)); // BaseEntity-dən
        }
    }
}
