using FluentValidation;

namespace Chinook.API.Features.Playlists;

public sealed class ReorderPlaylistCommandValidator : AbstractValidator<ReorderPlaylistCommand>
{
    public ReorderPlaylistCommandValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("PlaylistId is required.")
            .GreaterThan(0)
            .WithMessage("PlaylistId must be greater than 0.");

        RuleFor(x => x.TrackIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("TrackIds are required.")
            .NotEmpty()
            .WithMessage("At least one TrackId must be provided.");

        RuleForEach(x => x.TrackIds)
            .GreaterThan(0)
            .WithMessage("Each TrackId must be greater than 0.");
    }
}
