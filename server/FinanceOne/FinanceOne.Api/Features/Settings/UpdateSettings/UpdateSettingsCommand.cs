namespace FinanceOne.Api.Features.Settings.UpdateSettings;

public sealed record UpdateSettingsCommand(int PeriodStartDay) : IRequest<Response<Unit>>;
