using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class MonthlySavingConfiguration : IEntityTypeConfiguration<MonthlySaving>
{
    public void Configure(EntityTypeBuilder<MonthlySaving> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired();

        builder.Property(m => m.Amount)
            .HasPrecision(18, 2);

        builder.Property(m => m.RecurrenceDay)
            .IsRequired();

        // Deletion of a referenced SavingGoal is blocked in the application layer (see
        // Features/SavingGoals/DeleteSavingGoal) rather than cascaded, so this stays Restrict.
        builder.HasOne(m => m.SavingGoal)
            .WithMany(s => s.MonthlySavings)
            .HasForeignKey(m => m.SavingGoalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
