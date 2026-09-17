using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;

public sealed record GetSavingGoalImageQuery(Guid Id) : IRequest<Response<DownloadedBlob>>;
