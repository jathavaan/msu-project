namespace FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

public sealed record CategorizationRuleVm(Guid Id, string Keyword, Guid CategoryId, string CategoryName);
