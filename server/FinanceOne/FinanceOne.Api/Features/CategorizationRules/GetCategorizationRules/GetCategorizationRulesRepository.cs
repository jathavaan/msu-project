using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

public interface IGetCategorizationRulesRepository
{
    Task<List<CategorizationRuleVm>> GetCategorizationRules(CancellationToken cancellationToken);
}

public sealed class GetCategorizationRulesRepository(FinanceOneDbContext context) : IGetCategorizationRulesRepository
{
    public Task<List<CategorizationRuleVm>> GetCategorizationRules(CancellationToken cancellationToken) =>
        context.CategorizationRules
            .OrderBy(r => r.Keyword)
            .Select(r => new CategorizationRuleVm(r.Id, r.Keyword, r.CategoryId, r.Category!.Name))
            .ToListAsync(cancellationToken);
}
