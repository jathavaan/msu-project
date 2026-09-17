namespace FinanceOne.Api.Features.Settings.GetSettings;

public sealed class GetSettingsHandler(IGetSettingsRepository repository)
    : IRequestHandler<GetSettingsQuery, Response<SettingsVm>>
{
    // No settings row yet means nothing has been configured — default to a plain calendar month
    // rather than treating it as an error. UpdateSettings creates the row on first write.
    private const int DefaultPeriodStartDay = 1;

    public async Task<Response<SettingsVm>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var periodStartDay = await repository.GetPeriodStartDay(cancellationToken);
        return Response<SettingsVm>.Success(new SettingsVm(periodStartDay ?? DefaultPeriodStartDay));
    }
}
