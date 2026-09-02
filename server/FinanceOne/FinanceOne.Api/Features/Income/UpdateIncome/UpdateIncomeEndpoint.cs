namespace FinanceOne.Api.Features.Income.UpdateIncome;

public static class UpdateIncomeEndpoint
{
    public static RouteGroupBuilder MapUpdateIncome(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateIncomeCommand command, UpdateIncomeHandler handler, CancellationToken ct) =>
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
            .AddEndpointFilter<ValidationFilter<UpdateIncomeCommand>>();

        return group;
    }
}
