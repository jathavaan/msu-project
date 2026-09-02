namespace FinanceOne.Api.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdHandler(IGetCategoryByIdRepository repository)
    : IRequestHandler<GetCategoryByIdQuery, Response<CategoryVm>>
{
    public async Task<Response<CategoryVm>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await repository.GetById(request.Id, cancellationToken);
        return category is null
            ? Response<CategoryVm>.Failure(StatusCodes.Status404NotFound, "Category not found.")
            : Response<CategoryVm>.Success(category);
    }
}
