using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class LessonItemConfiguration : IEntityTypeConfiguration<LessonItem>
    {
        public void Configure(EntityTypeBuilder<LessonItem> builder)
        {
            
            builder.HasKey(l => l.Id);

            
            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Order)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(l => l.LessonType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(l => l.VideoUrl)
                .HasMaxLength(500);

            builder.Property(l => l.ReadingText)
                .HasMaxLength(10000);

            builder.Property(l => l.EstimatedMinutes)
                .HasDefaultValue(null);

            
            builder.Property(l => l.VideoDurationSeconds)
                .HasDefaultValue(null);

            builder.Property(l => l.IsPreviewable)
                .HasDefaultValue(false);

            
            builder.HasIndex(l => new { l.ModuleId, l.Order });

           
            builder.HasOne(l => l.Module)
                .WithMany(m => m.LessonItems)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(l => l.Quiz)
                .WithOne(q => q.LessonItem)
                .HasForeignKey<Quiz>(q => q.LessonItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
