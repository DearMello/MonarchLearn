using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Enrollments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Domain.Entities.LearningProgress
{
    public class Certificate : SoftDeletableEntity
    {
        
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        public string OwnerFullName { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? CertificateUrl { get; set; }
    }
}
