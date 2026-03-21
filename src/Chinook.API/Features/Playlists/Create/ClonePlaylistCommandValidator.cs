using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class ClonePlaylistCommandValidator : AbstractValidator<ClonePlaylistCommand>
{
    public ClonePlaylistCommandValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");

        RuleFor(x => x.Name)
            .MaximumLength(120)
            .WithMessage("Playlist name must not exceed 120 characters.")
            .When(x => x.Name is not null);
    }
}
