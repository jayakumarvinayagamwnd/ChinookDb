using FluentValidation;

namespace Chinook.API.Features.Analytics;

public sealed class GetPlaylistEngagementQueryValidator : AbstractValidator<GetPlaylistEngagementQuery>
{
    public GetPlaylistEngagementQueryValidator()
    {
        RuleFor(x => x.PlaylistId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("PlaylistId is required.")
            .GreaterThan(0).WithMessage("PlaylistId must be greater than 0.");
    }
}
