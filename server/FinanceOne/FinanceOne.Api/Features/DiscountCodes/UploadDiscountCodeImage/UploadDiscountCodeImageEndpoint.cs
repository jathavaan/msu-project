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
        })
        // Minimal APIs auto-require antiforgery validation for any endpoint binding an IFormFile,
        // but this API has no antiforgery tokens/middleware anywhere (it's a stateless JSON API
        // called from the SPA over fetch) — without this, every call 500s before reaching the handler.
        .DisableAntiforgery();

        return group;
    }
}
