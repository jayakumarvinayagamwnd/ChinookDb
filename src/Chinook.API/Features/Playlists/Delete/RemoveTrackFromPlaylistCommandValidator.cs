using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class RemoveTrackFromPlaylistCommandValidator : AbstractValidator<RemoveTrackFromPlaylistCommand>
{
    public RemoveTrackFromPlaylistCommandValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");

        RuleFor(x => x.TrackId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("TrackId is required.")
            .GreaterThan(0)
            .WithMessage("TrackId must be greater than 0.");
    }
}
