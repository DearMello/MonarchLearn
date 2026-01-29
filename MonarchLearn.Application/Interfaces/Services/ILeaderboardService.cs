using MonarchLearn.Application.DTOs.Gamification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ILeaderboardService
    {
        // Global liderlər cədvəlini gətirir (Top N user)
        Task<List<LeaderboardUserDto>> GetGlobalLeaderboardAsync(int topCount);

        // Bir istifadəçinin öz qlobal reytinqini (rankını) gətirir
        Task<UserRankDto> GetUserRankAsync(int userId);
    }
}
