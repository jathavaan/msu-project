namespace FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;

public static class DeleteMonthlySavingEndpoint
{
    public static RouteGroupBuilder MapDeleteMonthlySaving(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteMonthlySavingHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteMonthlySavingCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
