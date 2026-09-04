namespace FinanceOne.Api.Features.MonthlySavings.GetMonthlySavingById;

public sealed class GetMonthlySavingByIdHandler(IGetMonthlySavingByIdRepository repository)
    : IRequestHandler<GetMonthlySavingByIdQuery, Response<MonthlySavingVm>>
{
    public async Task<Response<MonthlySavingVm>> Handle(GetMonthlySavingByIdQuery request, CancellationToken cancellationToken)
    {
        var monthlySaving = await repository.GetById(request.Id, cancellationToken);
        return monthlySaving is null
            ? Response<MonthlySavingVm>.Failure(StatusCodes.Status404NotFound, "Monthly saving not found.")
            : Response<MonthlySavingVm>.Success(monthlySaving);
    }
}
