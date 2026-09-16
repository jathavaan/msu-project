namespace FinanceOne.Api.Features.DiscountCodes.GetDiscountCodeImage;

public static class GetDiscountCodeImageEndpoint
{
    public static RouteGroupBuilder MapGetDiscountCodeImage(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}/image", async (Guid id, GetDiscountCodeImageHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetDiscountCodeImageQuery(id), ct);
            return response.IsSuccess
                ? Results.Stream(response.Result!.Content, response.Result.ContentType)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
