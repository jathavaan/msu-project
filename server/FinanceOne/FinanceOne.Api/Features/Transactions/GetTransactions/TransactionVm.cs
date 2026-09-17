namespace FinanceOne.Api.Features.Transactions.GetTransactions;

public sealed record TransactionVm(
    Guid Id,
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid? CategoryId,
    string? CategoryName);
