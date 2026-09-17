namespace FinanceOne.Api.Features.Transactions.GetTransactions;

public sealed record GetTransactionsQuery(Guid? CategoryId, bool UncategorizedOnly, DateOnly? From, DateOnly? To)
    : IRequest<Response<List<TransactionVm>>>;
