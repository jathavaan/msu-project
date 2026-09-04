namespace FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

public static class UpdateMonthlySavingEndpoint
{
    public static RouteGroupBuilder MapUpdateMonthlySaving(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateMonthlySavingCommand command, UpdateMonthlySavingHandler handler, CancellationToken ct) =>
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
            .AddEndpointFilter<ValidationFilter<UpdateMonthlySavingCommand>>();

        return group;
    }
}
