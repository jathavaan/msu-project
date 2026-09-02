namespace FinanceOne.Api.Features.Categories.GetCategories;

public sealed class GetCategoriesHandler(IGetCategoriesRepository repository)
    : IRequestHandler<GetCategoriesQuery, Response<List<CategoryVm>>>
{
    public async Task<Response<List<CategoryVm>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.GetCategories(request.Type, cancellationToken);
        return Response<List<CategoryVm>>.Success(categories);
    }
}
