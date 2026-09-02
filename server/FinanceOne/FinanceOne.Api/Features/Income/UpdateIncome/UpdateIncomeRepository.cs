using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Income.UpdateIncome;

public interface IUpdateIncomeRepository
{
    Task<Domain.Entites.Income?> GetById(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetIncomeCategory(Guid categoryId, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

public sealed class UpdateIncomeRepository(FinanceOneDbContext context) : IUpdateIncomeRepository
{
    public Task<Domain.Entites.Income?> GetById(Guid id, CancellationToken cancellationToken) =>
        context.Incomes.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<Category?> GetIncomeCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
