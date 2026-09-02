using FluentValidation;

namespace FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;

public sealed class UpdateDiscountCodeValidator : AbstractValidator<UpdateDiscountCodeCommand>
{
    public UpdateDiscountCodeValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.StoreName).NotEmpty();
        RuleFor(c => c.ExpiryDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Expiry date cannot be in the past.");
    }
}
