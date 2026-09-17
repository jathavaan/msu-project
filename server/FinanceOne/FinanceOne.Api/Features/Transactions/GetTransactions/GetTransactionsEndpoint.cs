namespace FinanceOne.Api.Features.Transactions.GetTransactions;

public static class GetTransactionsEndpoint
{
    public static RouteGroupBuilder MapGetTransactions(this RouteGroupBuilder group)
    {
        // uncategorizedOnly is bool? (not bool) so minimal API model binding treats it as
        // optional, the same way the other nullable query parameters here are — an unadorned
        // bool parameter is otherwise required and a request omitting it 400s.
        group.MapGet("/", async (Guid? categoryId, bool? uncategorizedOnly, DateOnly? from, DateOnly? to, GetTransactionsHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(
                new GetTransactionsQuery(categoryId, uncategorizedOnly ?? false, from, to), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
