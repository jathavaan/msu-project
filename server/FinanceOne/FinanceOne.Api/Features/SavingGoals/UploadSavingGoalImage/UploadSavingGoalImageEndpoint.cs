namespace FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;

public static class UploadSavingGoalImageEndpoint
{
    public static RouteGroupBuilder MapUploadSavingGoalImage(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}/image", async (Guid id, IFormFile file, UploadSavingGoalImageHandler handler, CancellationToken ct) =>
            {
                await using var stream = file.OpenReadStream();
                var response = await handler.Handle(
                    new UploadSavingGoalImageCommand(id, stream, file.ContentType, file.Length), ct);
                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            // Binding IFormFile makes ASP.NET Core mark this endpoint as requiring antiforgery
            // validation by default. This API has no cookie-based session/antiforgery token flow
            // (it's called programmatically, not from a browser form), so there's nothing for that
            // validation to check — without this, every request 500s with "contains anti-forgery
            // metadata, but a middleware was not found that supports anti-forgery".
            .DisableAntiforgery();

        return group;
    }
}
