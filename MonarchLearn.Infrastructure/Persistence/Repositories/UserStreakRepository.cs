using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Infrastructure.Persistence.Context;

using System.Linq;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
   
    public class UserStreakRepository : GenericRepository<UserStreak>, IUserStreakRepository
    {
        private readonly MonarchLearnDbContext _context;

        public UserStreakRepository(MonarchLearnDbContext context) : base(context)
        {
            _context = context;
        }
        

        public async Task<List<UserStreak>> GetTopStreaksAsync(int count)
        {
            
            return await _context.UserStreaks
                .OrderByDescending(s => s.CurrentStreakDays)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUserGlobalRankAsync(int userId)
        {
            
            var userStreak = await _context.UserStreaks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStreak == null) return 0;

            

            int higherStreaksCount = await _context.UserStreaks
                .CountAsync(s => s.CurrentStreakDays > userStreak.CurrentStreakDays);

            
            return higherStreaksCount + 1;
        }
    }
}