namespace FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

public static class GetCategorySpendTrendEndpoint
{
    public static RouteGroupBuilder MapGetCategorySpendTrend(this RouteGroupBuilder group)
    {
        group.MapGet("/{categoryId:guid}", async (Guid categoryId, GetCategorySpendTrendHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetCategorySpendTrendQuery(categoryId), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
