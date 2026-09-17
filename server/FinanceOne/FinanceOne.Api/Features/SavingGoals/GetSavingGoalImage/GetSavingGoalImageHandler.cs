using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.SavingGoals.GetSavingGoalImage;

public sealed class GetSavingGoalImageHandler(IGetSavingGoalImageRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<GetSavingGoalImageQuery, Response<DownloadedBlob>>
{
    public async Task<Response<DownloadedBlob>> Handle(GetSavingGoalImageQuery request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<DownloadedBlob>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        var blob = await blobStorage.DownloadAsync(BlobContainers.SavingGoalImages, request.Id.ToString(), cancellationToken);
        if (blob is null)
        {
            return Response<DownloadedBlob>.Failure(StatusCodes.Status404NotFound, "No image has been uploaded for this saving goal.");
        }

        return Response<DownloadedBlob>.Success(blob);
    }
}
