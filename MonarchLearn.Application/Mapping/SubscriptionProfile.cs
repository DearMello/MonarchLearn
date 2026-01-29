using AutoMapper;
using MonarchLearn.Application.DTOs.Subscriptions;
using MonarchLearn.Domain.Entities.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class SubscriptionProfile : Profile
    {
        public SubscriptionProfile()
        {
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>();

            CreateMap<UserSubscription, UserSubscriptionDto>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.SubscriptionPlan.Name))
                .ForMember(dest => dest.PricePaid, opt => opt.MapFrom(src => src.SubscriptionPlan.Price))
                // Kritik: Bu sahələr servisdə (manual) hesablanacaq
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.DaysRemaining, opt => opt.Ignore());
        }
    }
}
