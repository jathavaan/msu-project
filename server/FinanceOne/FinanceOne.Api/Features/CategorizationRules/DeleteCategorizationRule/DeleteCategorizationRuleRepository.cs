using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;

public interface IDeleteCategorizationRuleRepository
{
    Task<CategorizationRule?> GetById(Guid id, CancellationToken cancellationToken);
    Task Delete(CategorizationRule rule, CancellationToken cancellationToken);
}

public sealed class DeleteCategorizationRuleRepository(FinanceOneDbContext context) : IDeleteCategorizationRuleRepository
{
    public Task<CategorizationRule?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.CategorizationRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task Delete(CategorizationRule rule, CancellationToken cancellationToken)
    {
        context.CategorizationRules.Remove(rule);
        await context.SaveChangesAsync(cancellationToken);
    }
}
