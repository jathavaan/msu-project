namespace FinanceOne.Api.Features.Categories.CreateCategory;

public static class CreateCategoryEndpoint
{
    public static RouteGroupBuilder MapCreateCategory(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateCategoryCommand command, CreateCategoryHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/categories/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateCategoryCommand>>();

        return group;
    }
}
