namespace FinanceOne.Api.Features.Transactions.UpdateTransactionCategory;

public sealed class UpdateTransactionCategoryHandler(IUpdateTransactionCategoryRepository repository)
    : IRequestHandler<UpdateTransactionCategoryCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateTransactionCategoryCommand request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetById(request.Id, cancellationToken);
        if (transaction is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Transaction not found.");
        }

        if (request.CategoryId is not null)
        {
            var category = await repository.GetCategory(request.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Category not found.");
            }
        }

        transaction.CategoryId = request.CategoryId;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
