using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.Subscriptions;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(MonarchLearnDbContext context) : base(context) { }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(int userId)
        {
            var now = DateTime.UtcNow;

            return await _context.UserSubscriptions
                .Include(us => us.SubscriptionPlan)
                .FirstOrDefaultAsync(us =>
                    us.UserId == userId &&
                    !us.IsDeleted &&  // Silinmişlər gəlməsin
                    us.StartDate <= now &&
                    us.EndDate >= now);
        }
    }
}