using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IUserStreakRepository : IGenericRepository<UserStreak>
    {
       
        Task<List<UserStreak>> GetTopStreaksAsync(int count);

        
        Task<int> GetUserGlobalRankAsync(int userId);
    }
}
