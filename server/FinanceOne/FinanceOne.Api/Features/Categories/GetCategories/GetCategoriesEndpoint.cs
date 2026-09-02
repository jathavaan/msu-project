namespace FinanceOne.Api.Features.Categories.GetCategories;

public static class GetCategoriesEndpoint
{
    public static RouteGroupBuilder MapGetCategories(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (CategoryType? type, GetCategoriesHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetCategoriesQuery(type), ct);
            return Results.Ok(response);
        });

        return group;
    }
}
