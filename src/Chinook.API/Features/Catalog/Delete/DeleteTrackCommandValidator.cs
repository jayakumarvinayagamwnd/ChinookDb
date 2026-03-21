using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class DeleteTrackCommandValidator : AbstractValidator<DeleteTrackCommand>
{
    public DeleteTrackCommandValidator()
    {
        RuleFor(x => x.TrackId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("TrackId is required.")
            .GreaterThan(0)
            .WithMessage("TrackId must be greater than 0.");
    }
}
