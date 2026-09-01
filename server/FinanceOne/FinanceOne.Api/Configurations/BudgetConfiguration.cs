using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.MonthlyLimit)
            .HasPrecision(18, 2);

        // One-to-one, Budget as the dependent: giving HasForeignKey<Budget> makes EF Core
        // add a unique index on CategoryId automatically, enforcing "one budget per category".
        //
        // "Must reference an Expense-type category" is a business rule enforced in the
        // application layer (see Features/Budgets/CreateBudget), not the database.
        builder.HasOne(b => b.Category)
            .WithOne(c => c.Budget)
            .HasForeignKey<Budget>(b => b.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
