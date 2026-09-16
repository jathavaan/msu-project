namespace FinanceOne.Api.Features.DiscountCodes.UploadDiscountCodeImage;

public static class UploadDiscountCodeImageEndpoint
{
    public static RouteGroupBuilder MapUploadDiscountCodeImage(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}/image", async (Guid id, IFormFile file, UploadDiscountCodeImageHandler handler, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var response = await handler.Handle(
                new UploadDiscountCodeImageCommand(id, stream, file.ContentType, file.Length), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
