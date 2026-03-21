using FluentValidation;

namespace Chinook.API.Features.Employees;

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(40).WithMessage("FirstName must not exceed 40 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(20).WithMessage("LastName must not exceed 20 characters.");

        RuleFor(x => x.Title)
            .MaximumLength(30).WithMessage("Title must not exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.ReportsTo)
            .GreaterThan(0).WithMessage("ReportsTo must be greater than 0.")
            .When(x => x.ReportsTo.HasValue);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.HireDate)
            .GreaterThanOrEqualTo(x => x.BirthDate!.Value)
            .When(x => x.HireDate.HasValue && x.BirthDate.HasValue)
            .WithMessage("HireDate must be greater than or equal to BirthDate.");
    }
}
