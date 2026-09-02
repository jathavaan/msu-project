namespace FinanceOne.Api.Features.Income.CreateIncome;

public sealed class CreateIncomeHandler(ICreateIncomeRepository repository)
    : IRequestHandler<CreateIncomeCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetIncomeCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Income)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Income category not found.");
        }

        var income = new Domain.Entites.Income
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            RecurrenceDay = request.RecurrenceDay
        };
        var id = await repository.Add(income, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
