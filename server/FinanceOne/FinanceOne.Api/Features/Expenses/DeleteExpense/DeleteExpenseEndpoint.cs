namespace FinanceOne.Api.Features.Expenses.DeleteExpense;

public static class DeleteExpenseEndpoint
{
    public static RouteGroupBuilder MapDeleteExpense(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteExpenseHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteExpenseCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
