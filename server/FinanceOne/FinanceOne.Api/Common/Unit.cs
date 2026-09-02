namespace FinanceOne.Api.Common;

// Stand-in for "no payload" so commands that only signal success/failure
// can still return Response<Unit> instead of a special-cased void.
public readonly struct Unit;
