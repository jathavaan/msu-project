namespace FinanceOne.Api.Features.Categories.DeleteCategory;

public static class DeleteCategoryEndpoint
{
    public static RouteGroupBuilder MapDeleteCategory(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, DeleteCategoryHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new DeleteCategoryCommand(id), ct);
            return response.IsSuccess
                ? Results.NoContent()
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
