using MonarchLearn.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.Users
{
    public class UserEducation : BaseEntity
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public string SchoolName { get; set; }
        public string Degree { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? GraduationDate { get; set; }
    }


}
