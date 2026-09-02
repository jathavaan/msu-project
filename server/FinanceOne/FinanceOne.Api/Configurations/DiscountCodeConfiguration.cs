using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.StoreName)
            .IsRequired();

        // CodeText and CodeImageUrl are nullable by convention (string?).
    }
}
