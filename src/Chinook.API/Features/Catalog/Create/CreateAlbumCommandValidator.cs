using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class CreateAlbumCommandValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumCommandValidator()
    {
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
