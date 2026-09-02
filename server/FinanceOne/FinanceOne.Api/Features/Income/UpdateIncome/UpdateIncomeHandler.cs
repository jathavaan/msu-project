namespace FinanceOne.Api.Features.Income.UpdateIncome;

public sealed class UpdateIncomeHandler(IUpdateIncomeRepository repository)
    : IRequestHandler<UpdateIncomeCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateIncomeCommand request, CancellationToken cancellationToken)
    {
        var income = await repository.GetById(request.Id, cancellationToken);
        if (income is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Income not found.");
        }

        var category = await repository.GetIncomeCategory(request.CategoryId, cancellationToken);
        if (category is null || category.Type != CategoryType.Income)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Income category not found.");
        }

        income.Name = request.Name;
        income.Amount = request.Amount;
        income.CategoryId = request.CategoryId;
        income.RecurrenceDay = request.RecurrenceDay;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
