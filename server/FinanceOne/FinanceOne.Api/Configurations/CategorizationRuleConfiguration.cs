using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class CategorizationRuleConfiguration : IEntityTypeConfiguration<CategorizationRule>
{
    public void Configure(EntityTypeBuilder<CategorizationRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Keyword)
            .IsRequired();

        builder.HasOne(r => r.Category)
            .WithMany(c => c.CategorizationRules)
            .HasForeignKey(r => r.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
