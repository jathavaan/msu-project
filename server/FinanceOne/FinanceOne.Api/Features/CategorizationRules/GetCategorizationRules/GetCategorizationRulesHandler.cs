namespace FinanceOne.Api.Features.CategorizationRules.GetCategorizationRules;

public sealed class GetCategorizationRulesHandler(IGetCategorizationRulesRepository repository)
    : IRequestHandler<GetCategorizationRulesQuery, Response<List<CategorizationRuleVm>>>
{
    public async Task<Response<List<CategorizationRuleVm>>> Handle(GetCategorizationRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await repository.GetCategorizationRules(cancellationToken);
        return Response<List<CategorizationRuleVm>>.Success(rules);
    }
}
