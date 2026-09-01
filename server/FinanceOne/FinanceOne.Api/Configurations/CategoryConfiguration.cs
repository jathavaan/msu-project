using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOne.Api.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        // Relationships to Income/Expense/Budget are configured from the
        // dependent side (IncomeConfiguration, ExpenseConfiguration, BudgetConfiguration).
    }
}
