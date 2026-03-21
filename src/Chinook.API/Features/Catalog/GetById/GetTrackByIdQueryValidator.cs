using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class GetTrackByIdQueryValidator : AbstractValidator<GetTrackByIdQuery>
{
    public GetTrackByIdQueryValidator()
    {
        RuleFor(x => x.TrackId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("TrackId is required.")
            .GreaterThan(0)
            .WithMessage("TrackId must be greater than 0.");
    }
}
