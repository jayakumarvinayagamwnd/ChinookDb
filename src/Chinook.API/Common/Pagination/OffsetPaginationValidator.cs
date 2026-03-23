using FluentValidation;

namespace Chinook.API.Common.Pagination;

public abstract class OffsetPaginationValidator<T> : AbstractValidator<T>
    where T : IOffsetPaginatedQuery
{
    protected OffsetPaginationValidator()
    {
        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset must be greater than or equal to 0.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, OffsetPaginationDefaults.MaxLimit)
            .WithMessage($"Limit must be between 1 and {OffsetPaginationDefaults.MaxLimit}.");
    }
}