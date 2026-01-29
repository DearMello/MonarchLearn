using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Subscriptions
{
    public class SubscriptionPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public decimal Price { get; set; }      
        public int DurationDays { get; set; }   
        
    }
}
