using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.LearningProgress;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
    {
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            
            builder.Property(c => c.OwnerFullName)
                   .HasMaxLength(200);

            

            builder.HasOne(c => c.Enrollment)       
                   .WithOne(e => e.Certificate)     
                   .HasForeignKey<Certificate>(c => c.EnrollmentId) 
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}