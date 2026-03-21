using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class UpdateAlbumCommandValidator : AbstractValidator<UpdateAlbumCommand>
{
    public UpdateAlbumCommandValidator()
    {
        RuleFor(x => x.AlbumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("AlbumId is required.")
            .GreaterThan(0)
            .WithMessage("AlbumId must be greater than 0.");

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Album title is required.")
            .MaximumLength(160)
            .WithMessage("Album title must not exceed 160 characters.");

        RuleFor(x => x.ArtistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ArtistId is required.")
            .GreaterThan(0)
            .WithMessage("ArtistId must be greater than 0.");
    }
}
