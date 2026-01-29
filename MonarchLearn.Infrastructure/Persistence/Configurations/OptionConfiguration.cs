using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Infrastructure.Persistence.Configurations
{
    public class OptionConfiguration : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            
            builder.HasKey(o => o.Id);

            
            builder.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.IsCorrect)
                .IsRequired()
                .HasDefaultValue(false);

            
            builder.Property(o => o.Order)
                .IsRequired()
                .HasDefaultValue(1);

            
            builder.HasIndex(o => new { o.QuestionId, o.Order });

            
        }
    }
}
