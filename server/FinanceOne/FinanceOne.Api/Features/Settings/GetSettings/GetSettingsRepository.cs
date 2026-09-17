using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Settings.GetSettings;

public interface IGetSettingsRepository
{
    Task<int?> GetPeriodStartDay(CancellationToken cancellationToken);
}

public sealed class GetSettingsRepository(FinanceOneDbContext context) : IGetSettingsRepository
{
    public Task<int?> GetPeriodStartDay(CancellationToken cancellationToken) =>
        context.AppSettings.Select(s => (int?)s.PeriodStartDay).FirstOrDefaultAsync(cancellationToken);
}
