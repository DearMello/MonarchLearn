using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Subscriptions
{
    public class UserSubscriptionDto
    {
        public string PlanName { get; set; }
        public decimal PricePaid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }      
        public int DaysRemaining { get; set; }  
    }
}
