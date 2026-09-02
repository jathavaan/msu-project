namespace FinanceOne.Api.Common;

// Marker interface: a command/query declares what it returns.
public interface IRequest<TResponse>;

// TRequest = the command/query type (e.g. CreateCategoryCommand)
// TResponse = what Handle returns (e.g. Response<Guid>)
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
