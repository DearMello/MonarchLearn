using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Quizzes;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(200);

           
          
            builder.HasOne(q => q.LessonItem)      
                   .WithOne(l => l.Quiz)           
                   .HasForeignKey<Quiz>(q => q.LessonItemId) 
                   .OnDelete(DeleteBehavior.Cascade); // Dərs silinsə, Quiz də silinsin
        }
    }
}