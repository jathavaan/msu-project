namespace FinanceOne.Api.Features.Expenses.UpdateExpense;

public static class UpdateExpenseEndpoint
{
    public static RouteGroupBuilder MapUpdateExpense(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateExpenseCommand command, UpdateExpenseHandler handler, CancellationToken ct) =>
            {
                if (id != command.Id)
                {
                    return Results.BadRequest();
                }

                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<UpdateExpenseCommand>>();

        return group;
    }
}
