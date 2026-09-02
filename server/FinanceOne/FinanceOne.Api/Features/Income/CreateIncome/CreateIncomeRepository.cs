using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Income.CreateIncome;

public interface ICreateIncomeRepository
{
    Task<Category?> GetIncomeCategory(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> Add(Domain.Entites.Income income, CancellationToken cancellationToken);
}

public sealed class CreateIncomeRepository(FinanceOneDbContext context) : ICreateIncomeRepository
{
    public Task<Category?> GetIncomeCategory(Guid categoryId, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public async Task<Guid> Add(Domain.Entites.Income income, CancellationToken cancellationToken)
    {
        context.Incomes.Add(income);
        await context.SaveChangesAsync(cancellationToken);
        return income.Id;
    }
}
