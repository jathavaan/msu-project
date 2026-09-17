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
        });

        return group;
    }
}
