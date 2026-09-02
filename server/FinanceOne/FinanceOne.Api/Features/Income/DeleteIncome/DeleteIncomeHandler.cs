namespace FinanceOne.Api.Features.Income.DeleteIncome;

public sealed class DeleteIncomeHandler(IDeleteIncomeRepository repository)
    : IRequestHandler<DeleteIncomeCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteIncomeCommand request, CancellationToken cancellationToken)
    {
        var income = await repository.GetById(request.Id, cancellationToken);
        if (income is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Income not found.");
        }

        await repository.Delete(income, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
