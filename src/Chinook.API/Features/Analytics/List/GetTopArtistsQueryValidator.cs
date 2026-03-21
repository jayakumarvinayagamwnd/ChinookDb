using FluentValidation;

namespace Chinook.API.Features.Analytics;

public sealed class GetTopArtistsQueryValidator : AbstractValidator<GetTopArtistsQuery>
{
    public GetTopArtistsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");
    }
}
