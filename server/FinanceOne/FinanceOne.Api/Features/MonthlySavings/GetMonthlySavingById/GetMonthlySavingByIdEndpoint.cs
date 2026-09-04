namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

public static class GetMonthlySavingByIdEndpoint
{
    public static RouteGroupBuilder MapGetMonthlySavingById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, GetMonthlySavingByIdHandler handler, CancellationToken ct) =>
        {
            var response = await handler.Handle(new GetMonthlySavingByIdQuery(id), ct);
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
        });

        return group;
    }
}
