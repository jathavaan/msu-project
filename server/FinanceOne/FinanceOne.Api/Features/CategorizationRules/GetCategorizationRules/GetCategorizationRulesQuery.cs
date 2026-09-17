namespace FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

public sealed record GetCategorizationRulesQuery : IRequest<Response<List<CategorizationRuleVm>>>;
