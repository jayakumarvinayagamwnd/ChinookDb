using FluentValidation;

namespace Chinook.API.Features.Employees;

public sealed class UpdateEmployeeManagerCommandValidator : AbstractValidator<UpdateEmployeeManagerCommand>
{
    public UpdateEmployeeManagerCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("EmployeeId is required.")
            .GreaterThan(0).WithMessage("EmployeeId must be greater than 0.");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("ManagerId must be greater than 0.")
            .When(x => x.ManagerId.HasValue);

        RuleFor(x => x.ManagerId)
            .NotEqual(x => x.EmployeeId)
            .When(x => x.ManagerId.HasValue)
            .WithMessage("An employee cannot be their own manager.");
    }
}
