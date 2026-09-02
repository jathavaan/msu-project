namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public sealed class CreateSavingGoalHandler(ICreateSavingGoalRepository repository)
    : IRequestHandler<CreateSavingGoalCommand, Response<Guid>>
{
    public async Task<Response<Guid>> Handle(CreateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var savingGoal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TargetAmount = request.TargetAmount,
            TargetDate = request.TargetDate,
            CurrentAmount = 0
        };
        var id = await repository.Add(savingGoal, cancellationToken);
        return Response<Guid>.Success(id);
    }
}
