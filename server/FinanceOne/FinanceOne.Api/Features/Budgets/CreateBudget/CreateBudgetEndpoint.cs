namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public static class CreateBudgetEndpoint
{
    public static RouteGroupBuilder MapCreateBudget(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateBudgetCommand command, CreateBudgetHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/budgets/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateBudgetCommand>>();

        return group;
    }
}
