namespace FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;

public sealed record DeleteCategorizationRuleCommand(Guid Id) : IRequest<Response<Unit>>;
