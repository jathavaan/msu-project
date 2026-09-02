namespace FinanceOne.Api.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler(IUpdateCategoryRepository repository)
    : IRequestHandler<UpdateCategoryCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetById(request.Id, cancellationToken);
        if (category is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Category not found.");
        }

        if (await repository.ExistsWithNameAndType(request.Id, request.Name, request.Type, cancellationToken))
        {
            return Response<Unit>.Failure(StatusCodes.Status409Conflict, "A category with this name and type already exists.");
        }

        category.Name = request.Name;
        category.Type = request.Type;
        await repository.Update(category, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
