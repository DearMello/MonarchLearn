using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Gamification
{
    public class UserStreakDto
    {
        public int UserId { get; set; }
        public int CurrentStreakDays { get; set; }
        public DateTime? LastActiveDate { get; set; }

        
        public string Message { get; set; }
    }
}
