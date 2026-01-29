using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Subscriptions
{
    public class CreatePlanDto
    {
        public string Name { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
    }
}
