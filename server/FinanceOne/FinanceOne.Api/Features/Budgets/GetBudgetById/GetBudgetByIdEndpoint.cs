namespace FinanceOne.Api.Features.Budgets.GetBudgetById;

public static class GetBudgetByIdEndpoint
{
    public static RouteGroupBuilder MapGetBudgetById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetBudgetByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetBudgetByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
