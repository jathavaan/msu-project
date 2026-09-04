namespace FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

public static class CreateMonthlySavingEndpoint
{
    public static RouteGroupBuilder MapCreateMonthlySaving(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateMonthlySavingCommand command, CreateMonthlySavingHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/monthly-savings/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateMonthlySavingCommand>>();

        return group;
    }
}
