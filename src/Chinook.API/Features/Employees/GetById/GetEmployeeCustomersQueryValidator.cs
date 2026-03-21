using FluentValidation;

namespace Chinook.API.Features.Employees;

public sealed class GetEmployeeCustomersQueryValidator : AbstractValidator<GetEmployeeCustomersQuery>
{
    public GetEmployeeCustomersQueryValidator()
    {
        RuleFor(x => x.EmployeeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("EmployeeId is required.")
            .GreaterThan(0).WithMessage("EmployeeId must be greater than 0.");
    }
}
