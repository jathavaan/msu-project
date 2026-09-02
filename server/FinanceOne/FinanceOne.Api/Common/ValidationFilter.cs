using FluentValidation;

namespace FinanceOne.Api.Common;

// Wired via .AddEndpointFilter<ValidationFilter<TRequest>>() on each mutating endpoint.
// If no IValidator<TRequest> is registered for a slice, this is a no-op — safe to add by default.
public sealed class ValidationFilter<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().First();
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(result.ToDictionary());
            }
        }

        return await next(context);
    }
}
