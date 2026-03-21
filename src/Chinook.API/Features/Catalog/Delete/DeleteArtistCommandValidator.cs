using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class DeleteArtistCommandValidator : AbstractValidator<DeleteArtistCommand>
{
    public DeleteArtistCommandValidator()
    {
        RuleFor(x => x.ArtistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ArtistId is required.")
            .GreaterThan(0)
            .WithMessage("ArtistId must be greater than 0.");
    }
}
