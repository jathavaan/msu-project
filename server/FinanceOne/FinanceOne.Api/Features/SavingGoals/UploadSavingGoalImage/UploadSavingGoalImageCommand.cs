namespace FinanceOne.Api.Features.SavingGoals.UploadSavingGoalImage;

// Carries an IFormFile's already-opened stream rather than the IFormFile itself, so the handler
// (and its unit tests) don't need to depend on ASP.NET Core's HTTP model binding types.
public sealed record UploadSavingGoalImageCommand(Guid Id, Stream Content, string? ContentType, long Length)
    : IRequest<Response<Unit>>;
