namespace FinanceOne.Api.Features.Settings.UpdateSettings;

public sealed class UpdateSettingsHandler(IUpdateSettingsRepository repository)
    : IRequestHandler<UpdateSettingsCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await repository.GetSettings(cancellationToken);
        if (settings is null)
        {
            await repository.Add(new AppSettings { Id = Guid.NewGuid(), PeriodStartDay = request.PeriodStartDay }, cancellationToken);
            return Response<Unit>.Success(new Unit());
        }

        settings.PeriodStartDay = request.PeriodStartDay;
        await repository.Update(cancellationToken);
        return Response<Unit>.Success(new Unit());
    }
}
