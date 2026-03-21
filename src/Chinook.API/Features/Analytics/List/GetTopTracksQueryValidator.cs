using FluentValidation;

namespace Chinook.API.Features.Analytics;

public sealed class GetTopTracksQueryValidator : AbstractValidator<GetTopTracksQuery>
{
    public GetTopTracksQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");
    }
}
