namespace FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

public sealed class CreateMonthlySavingHandler(ICreateMonthlySavingRepository repository)
    : IRequestHandler<CreateMonthlySavingCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateMonthlySavingCommand request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetSavingGoal(request.SavingGoalId, cancellationToken);
        if (savingGoal is null)
        {
            return Response<Guid>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        var monthlySaving = new Domain.Entites.MonthlySaving
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Amount = request.Amount,
            SavingGoalId = request.SavingGoalId,
            RecurrenceDay = request.RecurrenceDay
        };
        var id = await repository.Add(monthlySaving, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
