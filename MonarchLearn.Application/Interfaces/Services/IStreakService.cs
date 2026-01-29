using MonarchLearn.Application.DTOs.Gamification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IStreakService
    {
        Task<UserStreakDto> GetUserStreakAsync(int userId);

        // Bu metod yalnız günlük aktivlikdə çağırılır (Artırma məntiqi)
        Task UpdateStreakAsync(int userId);
    }

}
