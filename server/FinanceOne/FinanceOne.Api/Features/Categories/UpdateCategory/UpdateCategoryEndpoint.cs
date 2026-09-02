namespace FinanceOne.Api.Features.Categories.UpdateCategory;

public static class UpdateCategoryEndpoint
{
    public static RouteGroupBuilder MapUpdateCategory(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryCommand command, UpdateCategoryHandler handler, CancellationToken ct) =>
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
            .AddEndpointFilter<ValidationFilter<UpdateCategoryCommand>>();

        return group;
    }
}
