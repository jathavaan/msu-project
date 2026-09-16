using FinanceOne.Api.Common.BlobStorage;

namespace FinanceOne.Api.Features.SavingGoals.DeleteSavingGoal;

public sealed class DeleteSavingGoalHandler(IDeleteSavingGoalRepository repository, IBlobStorageService blobStorage)
    : IRequestHandler<DeleteSavingGoalCommand, Response<Unit>>
{
    public async Task<Response<Unit>> Handle(DeleteSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var savingGoal = await repository.GetById(request.Id, cancellationToken);
        if (savingGoal is null)
        {
            return Response<Unit>.Failure(StatusCodes.Status404NotFound, "Saving goal not found.");
        }

        if (await repository.IsReferenced(request.Id, cancellationToken))
        {
            return Response<Unit>.Failure(StatusCodes.Status409Conflict, "Saving goal is still referenced by a monthly saving.");
        }

        await repository.Delete(savingGoal, cancellationToken);

        // Images are kept indefinitely while the goal exists (no expiry-driven cleanup), but
        // deleting the goal itself should not leave an orphaned blob behind. No-op if the goal
        // never had an image uploaded through Upload Saving Goal Image.
        await blobStorage.DeleteAsync(BlobContainers.SavingGoalImages, request.Id.ToString(), cancellationToken);

        return Response<Unit>.Success(new Unit());
    }
}
