using MonarchLearn.Application.DTOs.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ISubscriptionService
    {
        // Bütün planları gətir (Pricing page)
        Task<List<SubscriptionPlanDto>> GetAllPlansAsync();

        // İstifadəçinin cari abunəliyi (Profile page)
        Task<UserSubscriptionDto?> GetUserCurrentSubscriptionAsync(int userId);

        // Plan almaq (Ödənişli və ya Free Trial)
        Task PurchaseSubscriptionAsync(int userId, int planId);

        Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto model);
        Task UpdatePlanAsync(UpdatePlanDto model);
        Task DeletePlanAsync(int planId);
    }
}
