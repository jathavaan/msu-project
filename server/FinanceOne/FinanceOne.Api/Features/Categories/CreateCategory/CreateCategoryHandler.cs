namespace FinanceOne.Api.Features.Categories.CreateCategory;

public sealed class CreateCategoryHandler(ICreateCategoryRepository repository)
    : IRequestHandler<CreateCategoryCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await repository.ExistsWithNameAndType(request.Name, request.Type, cancellationToken))
        {
            return Response<Guid>.Failure(StatusCodes.Status409Conflict, "A category with this name and type already exists.");
        }

        var category = new Category { Id = Guid.NewGuid(), Name = request.Name, Type = request.Type };
        var id = await repository.Add(category, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
