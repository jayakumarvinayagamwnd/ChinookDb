using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class CreateArtistCommandValidator : AbstractValidator<CreateArtistCommand>
{
    public CreateArtistCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Artist name is required.")
            .MaximumLength(120)
            .WithMessage("Artist name must not exceed 120 characters.");
    }
}
