namespace FinanceOne.Api.Features.MonthlySavings.DeleteMonthlySaving;

public sealed class DeleteMonthlySavingHandler(IDeleteMonthlySavingRepository repository)
    : IRequestHandler<DeleteMonthlySavingCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteMonthlySavingCommand request, CancellationToken cancellationToken)
    {
        var monthlySaving = await repository.GetById(request.Id, cancellationToken);
        if (monthlySaving is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Monthly saving not found.");
        }

        await repository.Delete(monthlySaving, cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
