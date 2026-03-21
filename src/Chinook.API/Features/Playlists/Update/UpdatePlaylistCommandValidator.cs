using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class UpdatePlaylistCommandValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistCommandValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Playlist name is required.")
            .MaximumLength(120)
            .WithMessage("Playlist name must not exceed 120 characters.");
    }
}
