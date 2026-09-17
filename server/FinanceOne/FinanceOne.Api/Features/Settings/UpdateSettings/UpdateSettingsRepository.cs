using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Settings.UpdateSettings;

public interface IUpdateSettingsRepository
{
    Task<AppSettings?> GetSettings(CancellationToken cancellationToken);
    Task Add(AppSettings settings, CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
}

// Upserts the single AppSettings row — Get/Add/Update rather than the usual GetById/Add/Update
// shape, since this slice always operates on "the one row" rather than one identified by an id.
public sealed class UpdateSettingsRepository(FinanceOneDbContext context) : IUpdateSettingsRepository
{
    public Task<AppSettings?> GetSettings(CancellationToken cancellationToken) =>
        context.AppSettings.FirstOrDefaultAsync(cancellationToken);

    public async Task Add(AppSettings settings, CancellationToken cancellationToken)
    {
        context.AppSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task Update(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
