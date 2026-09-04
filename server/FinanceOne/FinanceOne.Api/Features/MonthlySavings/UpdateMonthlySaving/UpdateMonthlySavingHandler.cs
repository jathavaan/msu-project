namespace FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

public sealed class UpdateMonthlySavingHandler(IUpdateMonthlySavingRepository repository)
    : IRequestHandler<UpdateMonthlySavingCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateMonthlySavingCommand request, CancellationToken cancellationToken)
    {
        var monthlySaving = await repository.GetById(request.Id, cancellationToken);
        if (monthlySaving is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Monthly saving not found.");
        }

        var savingGoal = await repository.GetSavingGoal(request.SavingGoalId, cancellationToken);
        if (savingGoal is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        monthlySaving.Name = request.Name;
        monthlySaving.Amount = request.Amount;
        monthlySaving.SavingGoalId = request.SavingGoalId;
        monthlySaving.RecurrenceDay = request.RecurrenceDay;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
