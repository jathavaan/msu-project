namespace FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

public static class CreateDiscountCodeEndpoint
{
    public static RouteGroupBuilder MapCreateDiscountCode(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateDiscountCodeCommand command, CreateDiscountCodeHandler handler, CancellationToken ct) =>
            {
                var response = await handler.Handle(command, ct);
                return response.IsSuccess
                    ? Results.Created($"/api/discount-codes/{response.Result}", response.Result)
                    : Results.Problem(statusCode: response.ErrorCode, detail: response.ErrorMessage);
            })
            .AddEndpointFilter<ValidationFilter<CreateDiscountCodeCommand>>();

        return group;
    }
}
