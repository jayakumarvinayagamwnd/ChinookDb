using FluentValidation;

namespace Chinook.API.Features.Customers;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CustomerId is required.")
            .GreaterThan(0).WithMessage("CustomerId must be greater than 0.");

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(40).WithMessage("First name must not exceed 40 characters.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(20).WithMessage("Last name must not exceed 20 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(60).WithMessage("Email must not exceed 60 characters.");

        RuleFor(x => x.Company)
            .MaximumLength(80).WithMessage("Company must not exceed 80 characters.")
            .When(x => x.Company is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(24).WithMessage("Phone must not exceed 24 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Fax)
            .MaximumLength(24).WithMessage("Fax must not exceed 24 characters.")
            .When(x => x.Fax is not null);

        RuleFor(x => x.SupportRepId)
            .GreaterThan(0).WithMessage("SupportRepId must be greater than 0.")
            .When(x => x.SupportRepId.HasValue);
    }
}
