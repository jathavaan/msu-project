namespace FinanceOne.Api.Features.Income.GetIncomeById;

public static class GetIncomeByIdEndpoint
{
    public static RouteGroupBuilder MapGetIncomeById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetIncomeByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetIncomeByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response.Result)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
