using AutoMapper;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;

namespace Chinook.API.Features.Analytics;

public sealed class AnalyticsMappingProfile : Profile
{
    public AnalyticsMappingProfile()
    {
        CreateProjection<Track, TopTrackBaseProjection>()
            .ForCtorParam(nameof(TopTrackBaseProjection.TrackId), opt => opt.MapFrom(src => src.TrackId))
            .ForCtorParam(nameof(TopTrackBaseProjection.TrackName), opt => opt.MapFrom(src => src.Name));
    }
}

public sealed record TopTrackBaseProjection(int TrackId, string TrackName);
