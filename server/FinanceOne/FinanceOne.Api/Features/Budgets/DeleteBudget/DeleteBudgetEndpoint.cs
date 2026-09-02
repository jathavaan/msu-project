namespace FinanceOne.Api.Features.Budgets.DeleteBudget;

public static class DeleteBudgetEndpoint
{
    public static RouteGroupBuilder MapDeleteBudget(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteBudgetHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteBudgetCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
