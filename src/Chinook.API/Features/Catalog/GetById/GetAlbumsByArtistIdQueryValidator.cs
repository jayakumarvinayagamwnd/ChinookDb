using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class GetAlbumsByArtistIdQueryValidator : AbstractValidator<GetAlbumsByArtistIdQuery>
{
    public GetAlbumsByArtistIdQueryValidator()
    {
        RuleFor(x => x.ArtistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ArtistId is required.")
            .GreaterThan(0)
            .WithMessage("ArtistId must be greater than 0.");
    }
}
