using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Subscriptions
{
    public class UserSubscription : SoftDeletableEntity
    {
        
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
