using MonarchLearn.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Subscriptions
{
    public class SubscriptionPlan : SoftDeletableEntity
    {
        
        public string Name { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }

}
