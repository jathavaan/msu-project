using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

public interface ICreateCategorizationRuleRepository
{
    Task<Category?> GetCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> Add(CategorizationRule rule, CancellationToken cancellationToken);
}

public sealed class CreateCategorizationRuleRepository(FinanceOneDbContext context) : ICreateCategorizationRuleRepository
{
    public Task<Category?> GetCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public async Task<Guid> Add(CategorizationRule rule, CancellationToken cancellationToken)
    {
        context.CategorizationRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }
}
