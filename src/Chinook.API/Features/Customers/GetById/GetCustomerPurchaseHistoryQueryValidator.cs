using FluentValidation;

namespace Chinook.API.Features.Customers;

public sealed class GetCustomerPurchaseHistoryQueryValidator : AbstractValidator<GetCustomerPurchaseHistoryQuery>
{
    public GetCustomerPurchaseHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("CustomerId is required.")
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than 0.");
    }
}
