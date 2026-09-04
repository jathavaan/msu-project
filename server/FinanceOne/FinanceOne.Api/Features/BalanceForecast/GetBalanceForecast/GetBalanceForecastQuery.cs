namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public sealed record GetBalanceForecastQuery : IRequest<Response<List<BalanceForecastPointVm>>>;
