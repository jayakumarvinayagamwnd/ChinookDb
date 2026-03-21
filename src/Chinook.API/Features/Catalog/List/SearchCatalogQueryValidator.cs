using FluentValidation;

namespace Chinook.API.Features.Catalog;

public sealed class SearchCatalogQueryValidator : AbstractValidator<SearchCatalogQuery>
{
    private static readonly HashSet<string> SupportedTypes = new(["artist", "album", "track"]);

    public SearchCatalogQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Search term is required.")
            .MaximumLength(120)
            .WithMessage("Search term must not exceed 120 characters.");

        RuleFor(x => x.Type)
            .Must(BeValidTypeFilter)
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("Type must contain only artist, album, or track values.");
    }

    private static bool BeValidTypeFilter(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return true;

        var values = type.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .ToList();

        return values.Count != 0 && values.All(SupportedTypes.Contains);
    }
}
