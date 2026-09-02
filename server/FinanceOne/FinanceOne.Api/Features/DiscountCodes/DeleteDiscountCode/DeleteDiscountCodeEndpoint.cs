namespace FinanceOne.Api.Features.DiscountCodes.DeleteDiscountCode;

public static class DeleteDiscountCodeEndpoint
{
    public static RouteGroupBuilder MapDeleteDiscountCode(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteDiscountCodeHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteDiscountCodeCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
