using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class CreateTrackCommandValidator : AbstractValidator<CreateTrackCommand>
{
    public CreateTrackCommandValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Track name is required.")
            .MaximumLength(200)
            .WithMessage("Track name must not exceed 200 characters.");

        RuleFor(x => x.AlbumId)
            .GreaterThan(0)
            .When(x => x.AlbumId.HasValue)
            .WithMessage("AlbumId must be greater than 0.");

        RuleFor(x => x.MediaTypeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("MediaTypeId is required.")
            .GreaterThan(0)
            .WithMessage("MediaTypeId must be greater than 0.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0)
            .When(x => x.GenreId.HasValue)
            .WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.Composer)
            .MaximumLength(220)
            .When(x => !string.IsNullOrWhiteSpace(x.Composer))
            .WithMessage("Composer must not exceed 220 characters.");

        RuleFor(x => x.Milliseconds)
            .GreaterThan(0)
            .WithMessage("Milliseconds must be greater than 0.");

        RuleFor(x => x.Bytes)
            .GreaterThan(0)
            .When(x => x.Bytes.HasValue)
            .WithMessage("Bytes must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("UnitPrice must be greater than 0.");
    }
}
