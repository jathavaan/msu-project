namespace FinanceOne.Api.Common.BlobStorage;

/// <summary>
/// Container names in the app's Blob Storage account (see infra/modules/storage-app.bicep). Every
/// container is private (no public access) and reached only through <see cref="IBlobStorageService"/>
/// — nothing outside the API talks to Blob Storage directly.
/// </summary>
public static class BlobContainers
{
    public const string Coupons = "coupons";
    public const string Exports = "exports";
    public const string StagedCsv = "staged-csv";
    public const string SavingGoalImages = "saving-goal-images";
}
