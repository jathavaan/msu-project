using Microsoft.EntityFrameworkCore;

namespace FinanceOne.IntegrationTests.Common;

/// <summary>
/// Base class for slice integration tests. Each test starts from an empty database and gets its own
/// DbContext, so nothing leaks between tests through EF's change tracker or through leftover rows.
/// </summary>
[Collection(DatabaseCollection.Name)]
public abstract class IntegrationTest : IAsyncLifetime
{
    private readonly MySqlFixture _fixture;

    protected IntegrationTest(MySqlFixture fixture)
    {
        _fixture = fixture;
        Context = fixture.CreateContext();
    }

    /// <summary>The context under test. Use <see cref="NewContext"/> to read back what was written.</summary>
    protected FinanceOneDbContext Context { get; }

    /// <summary>
    /// A second context over the same database. Asserting through this instead of <see cref="Context"/>
    /// forces a real round-trip to MySQL rather than a hit on the first context's change tracker,
    /// which is the only way a test can tell "saved" apart from "merely tracked".
    /// </summary>
    protected FinanceOneDbContext NewContext() => _fixture.CreateContext();

    public async Task InitializeAsync()
    {
        // Ordered so that dependents go before the rows they reference — every FK onto Category and
        // SavingGoal is Restrict, so the reverse order would be rejected.
        foreach (var table in (string[])
                 ["Budgets", "Expenses", "Incomes", "MonthlySavings", "SavingGoals", "DiscountCodes",
                     "Transactions", "CategorizationRules", "Categories"])
        {
            await Context.Database.ExecuteSqlRawAsync($"DELETE FROM `{table}`");
        }
    }

    public Task DisposeAsync() => Context.DisposeAsync().AsTask();

    protected async Task<Category> GivenCategory(string name, CategoryType type)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = name, Type = type };
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();
        return category;
    }

    protected async Task<Budget> GivenBudget(Guid categoryId, decimal monthlyLimit)
    {
        var budget = new Budget { Id = Guid.NewGuid(), CategoryId = categoryId, MonthlyLimit = monthlyLimit };
        Context.Budgets.Add(budget);
        await Context.SaveChangesAsync();
        return budget;
    }

    protected async Task<Expense> GivenExpense(Guid categoryId, string name, decimal amount, int recurrenceDay)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = name,
            Amount = amount,
            CategoryId = categoryId,
            RecurrenceDay = recurrenceDay,
        };
        Context.Expenses.Add(expense);
        await Context.SaveChangesAsync();
        return expense;
    }

    protected async Task<IncomeEntity> GivenIncome(Guid categoryId, string name, decimal amount, int recurrenceDay)
    {
        var income = new IncomeEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Amount = amount,
            CategoryId = categoryId,
            RecurrenceDay = recurrenceDay,
        };
        Context.Incomes.Add(income);
        await Context.SaveChangesAsync();
        return income;
    }

    protected async Task<SavingGoal> GivenSavingGoal(
        string name,
        decimal targetAmount,
        DateOnly targetDate,
        decimal currentAmount = 0m,
        decimal? interestRate = null)
    {
        var savingGoal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            Name = name,
            TargetAmount = targetAmount,
            TargetDate = targetDate,
            CurrentAmount = currentAmount,
            InterestRate = interestRate,
        };
        Context.SavingGoals.Add(savingGoal);
        await Context.SaveChangesAsync();
        return savingGoal;
    }

    protected async Task<MonthlySaving> GivenMonthlySaving(
        Guid savingGoalId,
        string name,
        decimal amount,
        int recurrenceDay)
    {
        var monthlySaving = new MonthlySaving
        {
            Id = Guid.NewGuid(),
            Name = name,
            Amount = amount,
            SavingGoalId = savingGoalId,
            RecurrenceDay = recurrenceDay,
        };
        Context.MonthlySavings.Add(monthlySaving);
        await Context.SaveChangesAsync();
        return monthlySaving;
    }

    protected async Task<DiscountCode> GivenDiscountCode(
        string storeName,
        DateOnly expiryDate,
        string? codeText = null,
        string? codeImageUrl = null)
    {
        var discountCode = new DiscountCode
        {
            Id = Guid.NewGuid(),
            StoreName = storeName,
            CodeText = codeText,
            CodeImageUrl = codeImageUrl,
            ExpiryDate = expiryDate,
        };
        Context.DiscountCodes.Add(discountCode);
        await Context.SaveChangesAsync();
        return discountCode;
    }

    protected async Task<Transaction> GivenTransaction(
        DateOnly date,
        string description,
        decimal amount,
        string hash,
        Guid? categoryId = null)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Date = date,
            Description = description,
            Amount = amount,
            CategoryId = categoryId,
            Hash = hash,
        };
        Context.Transactions.Add(transaction);
        await Context.SaveChangesAsync();
        return transaction;
    }

    protected async Task<CategorizationRule> GivenCategorizationRule(string keyword, Guid categoryId)
    {
        var rule = new CategorizationRule { Id = Guid.NewGuid(), Keyword = keyword, CategoryId = categoryId };
        Context.CategorizationRules.Add(rule);
        await Context.SaveChangesAsync();
        return rule;
    }
}
