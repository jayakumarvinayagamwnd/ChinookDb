using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class GetTracksByPlaylistIdQueryValidator : AbstractValidator<GetTracksByPlaylistIdQuery>
{
    public GetTracksByPlaylistIdQueryValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");
    }
}
