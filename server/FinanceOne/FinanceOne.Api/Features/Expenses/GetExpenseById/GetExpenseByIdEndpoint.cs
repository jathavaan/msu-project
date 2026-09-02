namespace FinanceOne.Api.Features.Expenses.GetExpenseById;

public static class GetExpenseByIdEndpoint
{
    public static RouteGroupBuilder MapGetExpenseById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetExpenseByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetExpenseByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
