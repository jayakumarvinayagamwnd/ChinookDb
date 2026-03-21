using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class UpdateArtistCommandValidator : AbstractValidator<UpdateArtistCommand>
{
    public UpdateArtistCommandValidator()
    {
        RuleFor(x => x.ArtistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ArtistId is required.")
            .GreaterThan(0)
            .WithMessage("ArtistId must be greater than 0.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Artist name is required.")
            .MaximumLength(120)
            .WithMessage("Artist name must not exceed 120 characters.");
    }
}
