namespace FinanceOne.Api.Features.Income.DeleteIncome;

public static class DeleteIncomeEndpoint
{
    public static RouteGroupBuilder MapDeleteIncome(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteIncomeHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteIncomeCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
