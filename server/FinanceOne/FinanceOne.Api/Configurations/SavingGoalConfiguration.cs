using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class SavingGoalConfiguration : IEntityTypeConfiguration<SavingGoal>
{
    public void Configure(EntityTypeBuilder<SavingGoal> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired();

        builder.Property(s => s.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.CurrentAmount)
            .HasPrecision(18, 2);

        // MySql.EntityFrameworkCore can't read a `date` column back into DateOnly directly
        // (MySqlDataReader.GetFieldValue<DateOnly> throws InvalidCastException — the reader only
        // produces DateTime for `date` columns). Route through DateTime explicitly so EF never
        // asks the reader for a native DateOnly value; the column stays `date`.
        builder.Property(s => s.TargetDate)
            .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d))
            .HasColumnType("date");
    }
}
