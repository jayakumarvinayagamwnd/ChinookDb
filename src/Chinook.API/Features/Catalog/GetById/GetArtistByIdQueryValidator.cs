using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class GetArtistByIdQueryValidator : AbstractValidator<GetArtistByIdQuery>
{
    public GetArtistByIdQueryValidator()
    {
        RuleFor(x => x.ArtistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ArtistId is required.")
            .GreaterThan(0)
            .WithMessage("ArtistId must be greater than 0.");
    }
}
