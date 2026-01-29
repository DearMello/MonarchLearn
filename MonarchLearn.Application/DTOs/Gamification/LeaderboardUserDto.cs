using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Gamification
{
    public class LeaderboardUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int Rank { get; set; } 
        public int CurrentStreakDays { get; set; } 
        public int TotalCompletedLessons { get; set; }

        public int CompletedCoursesCount { get; set; }
    }
}
