using MonarchLearn.Domain.Entities.Subscriptions;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IUserSubscriptionRepository: IGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveSubscriptionAsync(int userId);
    }
}
