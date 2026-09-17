using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Persistence;

// Cross-cutting infrastructure shared by every vertical slice under Features/.
// Relations/constraints live in Configurations/ (one IEntityTypeConfiguration<T> per entity),
// applied below via ApplyConfigurationsFromAssembly.
public sealed class FinanceOneDbContext(DbContextOptions<FinanceOneDbContext> options)
    : DbContext(options)
{
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<CategorizationRule> CategorizationRules => Set<CategorizationRule>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<MonthlySaving> MonthlySavings => Set<MonthlySaving>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceOneDbContext).Assembly);
    }
}
