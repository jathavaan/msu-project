namespace FinanceOne.Api.Features.CategorizationRules.DeleteCategorizationRule;

public sealed class DeleteCategorizationRuleHandler(IDeleteCategorizationRuleRepository repository)
    : IRequestHandler<DeleteCategorizationRuleCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteCategorizationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await repository.GetById(request.Id, cancellationToken);
        if (rule is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Categorization rule not found.");
        }

        await repository.Delete(rule, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
