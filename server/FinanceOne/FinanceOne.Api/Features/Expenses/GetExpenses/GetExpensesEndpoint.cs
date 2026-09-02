namespace FinanceOne.Api.Features.Expenses.GetExpenses;

public static class GetExpensesEndpoint
{
    public static RouteGroupBuilder MapGetExpenses(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (Guid? categoryId, GetExpensesHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetExpensesQuery(categoryId), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
