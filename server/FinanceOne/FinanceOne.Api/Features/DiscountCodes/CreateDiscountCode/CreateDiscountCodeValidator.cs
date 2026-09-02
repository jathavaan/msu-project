using FluentValidation;

namespace FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;

public sealed class CreateDiscountCodeValidator : AbstractValidator<CreateDiscountCodeCommand>
{
    public CreateDiscountCodeValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.StoreName).NotEmpty();
        RuleFor(c => c.ExpiryDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Expiry date cannot be in the past.");
    }
}
