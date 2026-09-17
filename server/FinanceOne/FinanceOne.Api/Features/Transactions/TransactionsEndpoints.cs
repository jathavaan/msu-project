using FinanceOne.Api.Features.Transactions.GetTransactions;
using FinanceOne.Api.Features.Transactions.ImportTransactions;
using FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

namespace FinanceOne.Api.Features.Transactions;

public static class TransactionsEndpoints
{
    public static void MapTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions").WithTags("Transactions");

        group.MapImportTransactions();
        group.MapGetTransactions();
        group.MapUpdateTransactionCategory();
    }
}
