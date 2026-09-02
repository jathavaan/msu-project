namespace FinanceOne.Api.Features.Income.CreateIncome;

public static class CreateIncomeEndpoint
{
    public static RouteGroupBuilder MapCreateIncome(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateIncomeCommand command, CreateIncomeHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/income/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateIncomeCommand>>();

        return group;
    }
}
