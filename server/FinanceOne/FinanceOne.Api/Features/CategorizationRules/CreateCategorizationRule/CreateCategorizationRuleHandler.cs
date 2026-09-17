namespace FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

public sealed class CreateCategorizationRuleHandler(ICreateCategorizationRuleRepository repository)
    : IRequestHandler<CreateCategorizationRuleCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateCategorizationRuleCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetCategory(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Category not found.");
        }

        var rule = new CategorizationRule
        {
            Id = Guid.NewGuid(),
            Keyword = request.Keyword,
            CategoryId = request.CategoryId
        };
        var id = await repository.Add(rule, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
