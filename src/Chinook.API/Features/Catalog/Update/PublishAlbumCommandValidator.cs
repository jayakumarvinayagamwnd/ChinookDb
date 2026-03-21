using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class PublishAlbumCommandValidator : AbstractValidator<PublishAlbumCommand>
{
    public PublishAlbumCommandValidator()
    {
        RuleFor(x => x.AlbumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("AlbumId is required.")
            .GreaterThan(0)
            .WithMessage("AlbumId must be greater than 0.");
    }
}
