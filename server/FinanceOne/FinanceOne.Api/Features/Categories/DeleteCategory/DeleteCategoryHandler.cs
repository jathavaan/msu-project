namespace FinanceOne.Api.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryHandler(IDeleteCategoryRepository repository)
    : IRequestHandler<DeleteCategoryCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetById(request.Id, cancellationToken);
        if (category is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Category not found.");
        }

        if (await repository.IsReferenced(request.Id, cancellationToken))
        {
            return Response<Unit>.Failure(StatusCodes.Status409Conflict, "Category is still referenced by income, expense, or budget records.");
        }

        await repository.Delete(category, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
