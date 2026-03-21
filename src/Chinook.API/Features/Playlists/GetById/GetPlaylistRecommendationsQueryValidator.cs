using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class GetPlaylistRecommendationsQueryValidator : AbstractValidator<GetPlaylistRecommendationsQuery>
{
    public GetPlaylistRecommendationsQueryValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");
    }
}
