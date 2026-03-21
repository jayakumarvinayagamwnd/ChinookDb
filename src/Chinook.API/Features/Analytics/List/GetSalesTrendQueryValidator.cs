using FluentValidation;

namespace Chinook.API.Features.Analytics;

public sealed class GetSalesTrendQueryValidator : AbstractValidator<GetSalesTrendQuery>
{
    public GetSalesTrendQueryValidator()
    {
        RuleFor(x => x.Interval)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Interval is required.")
            .Must(interval =>
            {
                var normalized = interval.Trim().ToLowerInvariant();
                return normalized is "day" or "month" or "year";
            })
            .WithMessage("Interval must be one of: day, month, year.");
    }
}
