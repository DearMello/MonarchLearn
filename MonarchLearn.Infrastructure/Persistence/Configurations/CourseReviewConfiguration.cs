using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Courses;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class CourseReviewConfiguration : IEntityTypeConfiguration<CourseReview>
    {
        public void Configure(EntityTypeBuilder<CourseReview> builder)
        {
            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            // Kurs silinərsə rəylərin silinməsi normaldır (Cascade).
            // Amma kursu da Soft Delete edəcəyik, ona görə bu da fiziki silinməyəcək.
            builder.HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Yorumun cavabları varsa, Ana yorum fiziki silinə bilməz (Restrict).
            // Soft Delete etdiyimiz üçün bu bizə problem yaratmır.
            builder.HasOne(r => r.ParentReview)
                .WithMany()
                .HasForeignKey(r => r.ParentReviewId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}