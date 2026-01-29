using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Common
{
    public class SoftDeletableEntity : BaseEntity, ISoftDeletable
    {
        public bool IsDeleted { get; set; }
    }
}
