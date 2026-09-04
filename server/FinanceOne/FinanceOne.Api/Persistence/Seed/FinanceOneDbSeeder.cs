using Bogus;
using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Persistence.Seed;

// Dev-only dummy data (~100+ rows spread across every table) so list endpoints have
// enough rows to exercise pagination against. Wired up in Program.cs behind
// app.Environment.IsDevelopment(). Idempotent: no-ops if Categories already has data.
//
// Amounts are in NOK and are scaled/correlated per category (e.g. Rent costs far more
// than a Netflix subscription) rather than drawn from one flat range, so the seeded
// data reads like a plausible household budget instead of random noise.
public static class FinanceOneDbSeeder
{
    private static readonly string[] IncomeCategoryNames =
        ["Salary", "Freelance", "Investments", "Gifts"];

    private static readonly string[] ExpenseCategoryNames =
    [
        "Food & Drinks", "Rent", "Utilities", "Transport", "Entertainment",
        "Subscriptions", "Shopping", "Health", "Education", "Insurance"
    ];

    // Per-category description pool + realistic NOK amount range for a single income/expense.
    private static readonly Dictionary<string, (string[] Descriptions, decimal Min, decimal Max)> IncomeProfiles = new()
    {
        ["Salary"] = (["Monthly Salary", "Overtime Pay", "Year-End Bonus"], 32_000m, 58_000m),
        ["Freelance"] = (["Freelance Project", "Consulting Fee", "Side Hustle"], 3_000m, 25_000m),
        ["Investments"] = (["Stock Dividend", "Interest Income", "Fund Payout"], 300m, 6_000m),
        ["Gifts"] = (["Birthday Gift", "Cash Gift", "Tax Refund"], 500m, 5_000m)
    };

    private static readonly Dictionary<string, (string[] Descriptions, decimal Min, decimal Max)> ExpenseProfiles = new()
    {
        ["Food & Drinks"] = (["Grocery Shopping", "Dinner Out", "Coffee", "Takeaway"], 100m, 1_200m),
        ["Rent"] = (["Monthly Rent"], 8_000m, 16_000m),
        ["Utilities"] = (["Electricity Bill", "Water Bill", "Internet Bill"], 300m, 2_500m),
        ["Transport"] = (["Fuel", "Bus Pass", "Train Ticket", "Parking"], 100m, 1_500m),
        ["Entertainment"] = (["Cinema Tickets", "Concert Ticket", "Bowling Night", "Streaming Rental"], 100m, 1_200m),
        ["Subscriptions"] = (["Netflix Subscription", "Spotify Subscription", "Gym Membership", "Phone Plan"], 99m, 799m),
        ["Shopping"] = (["New Shoes", "Clothing", "Electronics", "Home Decor"], 200m, 3_500m),
        ["Health"] = (["Doctor Visit", "Pharmacy", "Dentist Visit", "Physiotherapy"], 150m, 2_500m),
        ["Education"] = (["Online Course", "Textbooks", "Course Fee"], 300m, 6_000m),
        ["Insurance"] = (["Car Insurance", "Home Insurance", "Travel Insurance"], 300m, 1_800m)
    };

    // A monthly budget limit is a rolled-up total across a category's purchases, so it
    // sits well above any single expense's amount range above.
    private static readonly Dictionary<string, (decimal Min, decimal Max)> BudgetProfiles = new()
    {
        ["Food & Drinks"] = (3_000m, 6_000m),
        ["Rent"] = (9_000m, 16_000m),
        ["Utilities"] = (1_000m, 3_000m),
        ["Transport"] = (800m, 2_500m),
        ["Entertainment"] = (500m, 2_000m),
        ["Subscriptions"] = (300m, 1_000m),
        ["Shopping"] = (500m, 4_000m),
        ["Health"] = (300m, 3_000m),
        ["Education"] = (500m, 6_000m),
        ["Insurance"] = (300m, 2_000m)
    };

    // One entry per name in SavingGoalNames below — each goal's target amount is scaled
    // to what that goal would realistically cost/require in NOK.
    private static readonly Dictionary<string, (decimal Min, decimal Max)> SavingGoalProfiles = new()
    {
        ["Emergency Fund"] = (50_000m, 150_000m),
        ["New Car"] = (150_000m, 400_000m),
        ["Vacation to Japan"] = (25_000m, 60_000m),
        ["Home Down Payment"] = (300_000m, 700_000m),
        ["Wedding"] = (100_000m, 300_000m),
        ["New Laptop"] = (10_000m, 25_000m),
        ["Retirement Fund"] = (300_000m, 1_000_000m),
        ["House Renovation"] = (50_000m, 300_000m),
        ["Boat"] = (100_000m, 500_000m),
        ["Christmas Gifts"] = (2_000m, 10_000m)
    };

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

        // 5 incomes per category (20 total), each with a description and amount drawn
        // from that category's own profile so e.g. "Salary" never shows up with a
        // "Freelance Project" description or a NOK 500 amount.
        var incomes = incomeCategories
            .SelectMany((category, index) =>
            {
                var profile = IncomeProfiles[category.Name];
                return new Faker<Income>()
                    .UseSeed(1001 + index)
                    .RuleFor(i => i.Id, _ => Guid.NewGuid())
                    .RuleFor(i => i.Name, f => f.PickRandom(profile.Descriptions))
                    .RuleFor(i => i.Amount, f => f.Finance.Amount(profile.Min, profile.Max))
                    .RuleFor(i => i.CategoryId, _ => category.Id)
                    .RuleFor(i => i.RecurrenceDay, f => f.Random.Int(1, 28))
                    .Generate(5);
            })
            .ToList();

        // 5 expenses per category (50 total), same category-correlated approach as incomes.
        var expenses = expenseCategories
            .SelectMany((category, index) =>
            {
                var profile = ExpenseProfiles[category.Name];
                return new Faker<Expense>()
                    .UseSeed(1002 + index)
                    .RuleFor(e => e.Id, _ => Guid.NewGuid())
                    .RuleFor(e => e.Name, f => f.PickRandom(profile.Descriptions))
                    .RuleFor(e => e.Amount, f => f.Finance.Amount(profile.Min, profile.Max))
                    .RuleFor(e => e.CategoryId, _ => category.Id)
                    .RuleFor(e => e.RecurrenceDay, f => f.Random.Int(1, 28))
                    .Generate(5);
            })
            .ToList();

        // One budget per expense category (Budget.CategoryId is unique, and init-only,
        // so it's set directly in the object initializer rather than via Faker rules).
        var budgetFaker = new Faker { Random = new Randomizer(1003) };
        var budgets = expenseCategories
            .Select(category =>
            {
                var (min, max) = BudgetProfiles[category.Name];
                return new Budget
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    MonthlyLimit = budgetFaker.Finance.Amount(min, max)
                };
            })
            .ToList();

        // One goal per name (no repeats), target/current amounts scaled to the goal itself.
        var savingGoals = SavingGoalNames
            .Select((name, index) =>
            {
                var (min, max) = SavingGoalProfiles[name];
                var faker = new Faker { Random = new Randomizer(1004 + index) };
                var targetAmount = faker.Finance.Amount(min, max);
                return new SavingGoal
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    TargetAmount = targetAmount,
                    TargetDate = DateOnly.FromDateTime(faker.Date.Future(2)),
                    CurrentAmount = faker.Finance.Amount(0, targetAmount)
                };
            })
            .ToList();

        var discountCodes = new Faker<DiscountCode>()
            .UseSeed(1005)
            .RuleFor(d => d.Id, _ => Guid.NewGuid())
            .RuleFor(d => d.StoreName, f => f.Company.CompanyName())
            .RuleFor(d => d.CodeText, f => f.Random.Bool(0.8f) ? f.Commerce.Ean8() : null)
            .RuleFor(d => d.CodeImageUrl, (f, d) => d.CodeText is null ? f.Image.PicsumUrl() : null)
            .RuleFor(d => d.ExpiryDate, f => DateOnly.FromDateTime(f.Date.Future(1)))
            .Generate(15);

        // 1-2 monthly savings per goal, funding a portion of the goal's still-unmet amount
        // (TargetAmount - CurrentAmount) at a plausible monthly rate rather than a flat range.
        var monthlySavings = savingGoals
            .SelectMany((goal, index) =>
            {
                var remaining = Math.Max(goal.TargetAmount - goal.CurrentAmount, 500m);
                var faker = new Faker { Random = new Randomizer(1006 + index) };
                var count = faker.Random.Int(1, 2);
                return new Faker<MonthlySaving>()
                    .UseSeed(1006 + index)
                    .RuleFor(m => m.Id, _ => Guid.NewGuid())
                    .RuleFor(m => m.Name, f => f.PickRandom("Automatic Transfer", "Round-Up Savings", "Payday Transfer"))
                    .RuleFor(m => m.Amount, f => f.Finance.Amount(remaining * 0.02m, remaining * 0.1m))
                    .RuleFor(m => m.SavingGoalId, _ => goal.Id)
                    .RuleFor(m => m.RecurrenceDay, f => f.Random.Int(1, 28))
                    .Generate(count);
            })
            .ToList();

        context.Incomes.AddRange(incomes);
        context.Expenses.AddRange(expenses);
        context.Budgets.AddRange(budgets);
        context.SavingGoals.AddRange(savingGoals);
        context.MonthlySavings.AddRange(monthlySavings);
        context.DiscountCodes.AddRange(discountCodes);

        await context.SaveChangesAsync(cancellationToken);
    }
}
