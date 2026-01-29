using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Gamification
{
    public class UserRankDto
    {
        public int UserId { get; set; }
        public int GlobalRank { get; set; } 
        public int CurrentStreakDays { get; set; }
    }
}
