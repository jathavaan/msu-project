using Bogus;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Persistence.Seed;

// Dev-only dummy data (~100+ rows spread across every table) so list endpoints have
// enough rows to exercise pagination against. Wired up in Program.cs behind
// app.Environment.IsDevelopment(). Idempotent: no-ops if Categories already has data.
public static class FinanceOneDbSeeder
{
    private static readonly string[] IncomeCategoryNames =
        ["Salary", "Freelance", "Investments", "Gifts"];

    private static readonly string[] ExpenseCategoryNames =
    [
        "Food & Drinks", "Rent", "Utilities", "Transport", "Entertainment",
        "Subscriptions", "Shopping", "Health", "Education", "Insurance"
    ];

    private static readonly string[] IncomeDescriptions =
    [
        "Monthly Salary", "Freelance Project", "Stock Dividend", "Bonus Payment",
        "Interest Income", "Rental Income", "Side Hustle", "Consulting Fee",
        "Tax Refund", "Gift"
    ];

    private static readonly string[] ExpenseDescriptions =
    [
        "Grocery Shopping", "Netflix Subscription", "Electricity Bill", "Gym Membership",
        "Uber Ride", "Coffee", "Dinner Out", "Phone Bill", "Car Insurance", "New Shoes",
        "Online Course", "Doctor Visit", "Train Ticket", "Concert Ticket", "Home Repair"
    ];

    private static readonly string[] SavingGoalNames =
    [
        "Emergency Fund", "New Car", "Vacation to Japan", "Home Down Payment",
        "Wedding", "New Laptop", "Retirement Fund", "House Renovation",
        "Boat", "Christmas Gifts"
    ];

    public static async Task SeedAsync(FinanceOneDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var incomeCategories = IncomeCategoryNames
            .Select(name => new Category { Id = Guid.NewGuid(), Name = name, Type = CategoryType.Income })
            .ToList();

        var expenseCategories = ExpenseCategoryNames
            .Select(name => new Category { Id = Guid.NewGuid(), Name = name, Type = CategoryType.Expense })
            .ToList();

        context.Categories.AddRange(incomeCategories);
        context.Categories.AddRange(expenseCategories);

        var incomes = new Faker<Income>()
            .UseSeed(1001)
            .RuleFor(i => i.Id, _ => Guid.NewGuid())
            .RuleFor(i => i.Name, f => f.PickRandom(IncomeDescriptions))
            .RuleFor(i => i.Amount, f => f.Finance.Amount(500, 8000))
            .RuleFor(i => i.CategoryId, f => f.PickRandom(incomeCategories).Id)
            .RuleFor(i => i.RecurrenceDay, f => f.Random.Int(1, 28))
            .Generate(20);

        var expenses = new Faker<Expense>()
            .UseSeed(1002)
            .RuleFor(e => e.Id, _ => Guid.NewGuid())
            .RuleFor(e => e.Name, f => f.PickRandom(ExpenseDescriptions))
            .RuleFor(e => e.Amount, f => f.Finance.Amount(5, 2000))
            .RuleFor(e => e.CategoryId, f => f.PickRandom(expenseCategories).Id)
            .RuleFor(e => e.RecurrenceDay, f => f.Random.Int(1, 28))
            .Generate(50);

        // One budget per expense category (Budget.CategoryId is unique, and init-only,
        // so it's set directly in the object initializer rather than via Faker rules).
        var budgetAmounts = new Faker();
        budgetAmounts.Random = new Randomizer(1003);
        var budgets = expenseCategories
            .Select(category => new Budget
            {
                Id = Guid.NewGuid(),
                CategoryId = category.Id,
                MonthlyLimit = budgetAmounts.Finance.Amount(100, 1500)
            })
            .ToList();

        var savingGoals = new Faker<SavingGoal>()
            .UseSeed(1004)
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Name, f => f.PickRandom(SavingGoalNames))
            .RuleFor(s => s.TargetAmount, f => f.Finance.Amount(1000, 50000))
            .RuleFor(s => s.TargetDate, f => DateOnly.FromDateTime(f.Date.Future(2)))
            .RuleFor(s => s.CurrentAmount, (f, s) => f.Finance.Amount(0, s.TargetAmount))
            .Generate(10);

        var discountCodes = new Faker<DiscountCode>()
            .UseSeed(1005)
            .RuleFor(d => d.Id, _ => Guid.NewGuid())
            .RuleFor(d => d.StoreName, f => f.Company.CompanyName())
            .RuleFor(d => d.CodeText, f => f.Random.Bool(0.8f) ? f.Commerce.Ean8() : null)
            .RuleFor(d => d.CodeImageUrl, (f, d) => d.CodeText is null ? f.Image.PicsumUrl() : null)
            .RuleFor(d => d.ExpiryDate, f => DateOnly.FromDateTime(f.Date.Future(1)))
            .Generate(15);

        context.Incomes.AddRange(incomes);
        context.Expenses.AddRange(expenses);
        context.Budgets.AddRange(budgets);
        context.SavingGoals.AddRange(savingGoals);
        context.DiscountCodes.AddRange(discountCodes);

        await context.SaveChangesAsync(cancellationToken);
    }
}
