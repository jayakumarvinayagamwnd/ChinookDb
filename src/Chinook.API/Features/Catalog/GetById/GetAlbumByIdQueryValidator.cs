using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class GetAlbumByIdQueryValidator : AbstractValidator<GetAlbumByIdQuery>
{
    public GetAlbumByIdQueryValidator()
    {
        RuleFor(x => x.AlbumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("AlbumId is required.")
            .GreaterThan(0)
            .WithMessage("AlbumId must be greater than 0.");
    }
}
