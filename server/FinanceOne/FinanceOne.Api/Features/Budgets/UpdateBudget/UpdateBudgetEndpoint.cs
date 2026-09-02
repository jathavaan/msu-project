namespace FinanceOne.Api.Features.Budgets.UpdateBudget;

public static class UpdateBudgetEndpoint
{
    public static RouteGroupBuilder MapUpdateBudget(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateBudgetCommand command, UpdateBudgetHandler handler, CancellationToken ct) =>
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
            .AddEndpointFilter<ValidationFilter<UpdateBudgetCommand>>();

        return group;
    }
}
