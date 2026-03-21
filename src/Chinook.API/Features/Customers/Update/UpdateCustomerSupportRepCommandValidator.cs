using FluentValidation;

namespace Chinook.API.Features.Customers;

public sealed class UpdateCustomerSupportRepCommandValidator : AbstractValidator<UpdateCustomerSupportRepCommand>
{
    public UpdateCustomerSupportRepCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CustomerId is required.")
            .GreaterThan(0).WithMessage("CustomerId must be greater than 0.");

        RuleFor(x => x.SupportRepId)
            .GreaterThan(0).WithMessage("SupportRepId must be greater than 0.")
            .When(x => x.SupportRepId.HasValue);
    }
}
