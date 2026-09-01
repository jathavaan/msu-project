using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Persistence;

// Cross-cutting infrastructure shared by every vertical slice under Features/.
// Relations/constraints live in Configurations/ (one IEntityTypeConfiguration<T> per entity),
// applied below via ApplyConfigurationsFromAssembly.
public sealed class FinanceOneDbContext(DbContextOptions<FinanceOneDbContext> options)
    : DbContext(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceOneDbContext).Assembly);
    }
}
