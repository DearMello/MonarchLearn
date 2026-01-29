using MonarchLearn.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Users
{
    public class UserStreak : BaseEntity
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int CurrentStreakDays { get; set; }
        public DateTime LastActiveDate { get; set; }
    }


}
