// Mirrors FinanceOne.Api/Usings.cs, plus the ASP.NET Core implicit usings the Web SDK adds
// there but the plain SDK does not (StatusCodes, Results).
global using Microsoft.AspNetCore.Http;
global using Xunit;
global using FinanceOne.Api.Common;
global using FinanceOne.Api.Domain.Entites;
global using FinanceOne.Api.Domain.Enums;
global using FinanceOne.Api.Persistence;

// The Income entity and the Features.Income namespace share a name, so tests living under
// Features/Income/ need an unambiguous way to say "the entity".
global using IncomeEntity = FinanceOne.Api.Domain.Entites.Income;
