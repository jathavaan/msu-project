using FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;
using FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;
using FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

namespace FinanceOne.Api.Features.CategorizationRules;

public static class CategorizationRulesEndpoints
{
    public static void MapCategorizationRulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categorization-rules").WithTags("CategorizationRules");

        group.MapCreateCategorizationRule();
        group.MapGetCategorizationRules();
        group.MapDeleteCategorizationRule();
    }
}
