using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.Hash)
            .IsRequired()
            .HasMaxLength(64);

        // Enforces the dedup rule at the database too: a re-imported row that hashes the same as
        // one already stored can never be inserted twice, even from a racing concurrent import.
        builder.HasIndex(t => t.Hash)
            .IsUnique();

        // MySql.EntityFrameworkCore can't read a `date` column back into DateOnly directly
        // (MySqlDataReader.GetFieldValue<DateOnly> throws InvalidCastException — the reader only
        // produces DateTime for `date` columns). Route through DateTime explicitly so EF never
        // asks the reader for a native DateOnly value; the column stays `date`.
        builder.Property(t => t.Date)
            .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d))
            .HasColumnType("date");

        // Uncategorized transactions have CategoryId == null, so this FK is optional and left
        // alone (rather than deleted) if its category is ever removed.
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
