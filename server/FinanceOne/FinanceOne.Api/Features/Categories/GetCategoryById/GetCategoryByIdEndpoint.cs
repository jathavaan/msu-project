namespace FinanceOne.Api.Features.Categories.GetCategoryById;

public static class GetCategoryByIdEndpoint
{
    public static RouteGroupBuilder MapGetCategoryById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetCategoryByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetCategoryByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response.Result)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
