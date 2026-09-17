namespace FinanceOne.Api.Features.Transactions.GetTransactions;

public sealed class GetTransactionsHandler(IGetTransactionsRepository repository)
    : IRequestHandler<GetTransactionsQuery, Response<List<TransactionVm>>>
{
    public async Task<Response<List<TransactionVm>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await repository.GetTransactions(
            request.CategoryId, request.UncategorizedOnly, request.From, request.To, cancellationToken);
        return Response<List<TransactionVm>>.Success(transactions);
    }
}
