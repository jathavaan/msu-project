namespace FinanceOne.Api.Features.Expenses.CreateExpense;

public static class CreateExpenseEndpoint
{
    public static RouteGroupBuilder MapCreateExpense(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateExpenseCommand command, CreateExpenseHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/expenses/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateExpenseCommand>>();

        return group;
    }
}
