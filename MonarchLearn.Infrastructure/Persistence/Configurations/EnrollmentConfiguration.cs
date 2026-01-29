using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Enrollments;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // Eyni tələbə eyni kursa 2-ci dəfə yazıla bilməz (Composite Unique Key)
      
            builder.HasIndex(e => new { e.UserId, e.CourseId })
                   .HasFilter("IsDeleted = 0")  
                   .IsUnique();

            //  User silinərsə -> Enrollment silinsin (Cascade). 
            //  istifadəçi yoxdursa, onun qeydiyyatı da lazım deyil.
            builder.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            
            // Dərs silinərsə Enrollment partlamasın.
            builder.HasOne(e => e.LastLessonItem)
                .WithMany()
                .HasForeignKey(e => e.LastLessonItemId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}