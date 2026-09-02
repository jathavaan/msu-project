namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeById;

public static class GetDiscountCodeByIdEndpoint
{
    public static RouteGroupBuilder MapGetDiscountCodeById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetDiscountCodeByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetDiscountCodeByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
