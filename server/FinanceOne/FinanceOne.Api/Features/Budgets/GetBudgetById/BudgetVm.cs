namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public sealed record BudgetVm(Guid Id, Guid CategoryId, string CategoryName, decimal MonthlyLimit, decimal UsedThisMonth);
