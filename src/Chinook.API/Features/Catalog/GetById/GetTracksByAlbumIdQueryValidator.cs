using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class GetTracksByAlbumIdQueryValidator : AbstractValidator<GetTracksByAlbumIdQuery>
{
    public GetTracksByAlbumIdQueryValidator()
    {
        RuleFor(x => x.AlbumId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("AlbumId is required.")
            .GreaterThan(0)
            .WithMessage("AlbumId must be greater than 0.");
    }
}
