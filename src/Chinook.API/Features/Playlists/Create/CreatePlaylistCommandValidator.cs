using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class CreatePlaylistCommandValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Playlist name is required.")
            .MaximumLength(120)
            .WithMessage("Playlist name must not exceed 120 characters.");
    }
}
