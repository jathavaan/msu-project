namespace FinanceOne.Api.Domain.Entites;

public sealed class SavingGoal
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required decimal TargetAmount { get; set; }
    public required DateOnly TargetDate { get; set; }

    // Manually tracked progress towards TargetAmount. Defaults to 0 on create;
    // adjusted via UpdateSavingGoal. No dedicated "contribution" slice exists yet.
    public decimal CurrentAmount { get; set; }

    // Optional annual interest rate (percentage, e.g. 4.5 = 4.5%), compounded monthly by
    // GetSavingGoalProjection. Null/0 means no interest — projection is contribution-only.
    public decimal? InterestRate { get; set; }

    public string? ImageUrl { get; set; }

    public ICollection<MonthlySaving> MonthlySavings { get; init; } = [];
}
