namespace FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

public sealed record UpcomingPaymentVm(DateOnly Date, string Name, decimal Amount, CategoryType Type);
