using FluentValidation;

namespace Chinook.API.Features.Customers;

public sealed class UpdateCustomerContactPreferencesCommandValidator : AbstractValidator<UpdateCustomerContactPreferencesCommand>
{
    public UpdateCustomerContactPreferencesCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CustomerId is required.")
            .GreaterThan(0).WithMessage("CustomerId must be greater than 0.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(60).WithMessage("Email must not exceed 60 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(24).WithMessage("Phone must not exceed 24 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Fax)
            .MaximumLength(24).WithMessage("Fax must not exceed 24 characters.")
            .When(x => x.Fax is not null);
    }
}
