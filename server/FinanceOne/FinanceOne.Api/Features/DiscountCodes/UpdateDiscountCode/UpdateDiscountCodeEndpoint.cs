namespace FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

public static class UpdateDiscountCodeEndpoint
{
    public static RouteGroupBuilder MapUpdateDiscountCode(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateDiscountCodeCommand command, UpdateDiscountCodeHandler handler, CancellationToken ct) =>
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
            .AddEndpointFilter<ValidationFilter<UpdateDiscountCodeCommand>>();

        return group;
    }
}
