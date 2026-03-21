using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class ReclassifyTrackGenreCommandValidator : AbstractValidator<ReclassifyTrackGenreCommand>
{
    public ReclassifyTrackGenreCommandValidator()
    {
        RuleFor(x => x.TrackId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("TrackId is required.")
            .GreaterThan(0)
            .WithMessage("TrackId must be greater than 0.");

        RuleFor(x => x.GenreId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("GenreId is required.")
            .GreaterThan(0)
            .WithMessage("GenreId must be greater than 0.");
    }
}
