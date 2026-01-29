using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.LearningProgress;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            
            builder.HasOne(a => a.Attempt)
                   .WithMany(at => at.Answers)
                   .HasForeignKey(a => a.AttemptId)
                   .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(a => a.Question)
                   .WithMany()
                   .HasForeignKey(a => a.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Option silinərsə -> Cavab silinməsin (Restrict).
            // Cycle xətasını qırmaq üçün vacibdir.
            builder.HasOne(a => a.SelectedOption)
                   .WithMany()
                   .HasForeignKey(a => a.SelectedOptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}