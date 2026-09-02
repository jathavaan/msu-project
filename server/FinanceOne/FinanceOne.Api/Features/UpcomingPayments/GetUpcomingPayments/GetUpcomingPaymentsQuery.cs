namespace FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

public sealed record GetUpcomingPaymentsQuery(int? Days) : IRequest<Response<List<UpcomingPaymentVm>>>;
