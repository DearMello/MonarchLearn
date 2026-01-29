using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Statistics; // Namespace-ə diqqət

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.ShortDescription)
                .HasMaxLength(500);

            builder.Property(c => c.AverageRating)
                .HasDefaultValue(0.0)
                .HasColumnType("decimal(3,2)");

            builder.Property(c => c.ReviewCount)
                .HasDefaultValue(0);

           
            builder.HasOne(c => c.Instructor)
                .WithMany(u => u.CreatedCourses) 
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.HasOne(c => c.Statistics)
                .WithOne(s => s.Course)
                .HasForeignKey<CourseStatistics>(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Skills)
            .WithMany(s => s.Courses)
            .UsingEntity(j => j.ToTable("CourseSkills")); 
        }
    }
}