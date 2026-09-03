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

        // MySql.EntityFrameworkCore can't read a `date` column back into DateOnly directly
        // (MySqlDataReader.GetFieldValue<DateOnly> throws InvalidCastException — the reader only
        // produces DateTime for `date` columns). Route through DateTime explicitly so EF never
        // asks the reader for a native DateOnly value; the column stays `date`.
        builder.Property(d => d.ExpiryDate)
            .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d))
            .HasColumnType("date");
    }
}
