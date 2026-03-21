using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetTrackByIdQuery(int TrackId) : IResultQuery<TrackDto>, ICacheableQuery
{
    public string CacheKey => $"catalog:track:{TrackId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetTrackByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetTrackByIdQueryHandler> logger) : IResultQueryHandler<GetTrackByIdQuery, TrackDto>
{
    public async Task<Result<TrackDto>> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetTrackByIdQueryHandler.Handle] - Handling GetTrackByIdQuery for TrackId: {TrackId}", request.TrackId);

        var track = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => t.TrackId == request.TrackId)
            .ProjectTo<TrackDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            logger.LogWarning("[GetTrackByIdQueryHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        logger.LogInformation("[GetTrackByIdQueryHandler.Handle] - Successfully retrieved track for TrackId: {TrackId}", request.TrackId);
        return Result.Ok(track);
    }
}
