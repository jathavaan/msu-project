namespace FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

public sealed record CreateCategorizationRuleCommand(string Keyword, Guid CategoryId)
    : IRequest<Response<Guid>>;
