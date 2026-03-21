using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetPlaylistEngagementQuery(int PlaylistId) : IResultQuery<PlaylistEngagementDto>, ICacheableQuery
{
    public string CacheKey => $"analytics:playlist-engagement:{PlaylistId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetPlaylistEngagementQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetPlaylistEngagementQueryHandler> logger) : IResultQueryHandler<GetPlaylistEngagementQuery, PlaylistEngagementDto>
{
    public async Task<Result<PlaylistEngagementDto>> Handle(GetPlaylistEngagementQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetPlaylistEngagementQueryHandler.Handle] - Handling GetPlaylistEngagementQuery for PlaylistId: {PlaylistId}", request.PlaylistId);

        var playlist = await dbContext.Playlists
            .AsNoTracking()
            .Where(p => p.PlaylistId == request.PlaylistId)
            .Select(p => new { p.PlaylistId, p.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            logger.LogWarning("[GetPlaylistEngagementQueryHandler.Handle] - Playlist not found for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Fail($"Playlist with ID {request.PlaylistId} not found.");
        }

        var playlistTrackIds = await dbContext.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .Select(pt => pt.TrackId)
            .ToListAsync(cancellationToken);

        if (playlistTrackIds.Count == 0)
        {
            var emptyDto = new PlaylistEngagementDto(
                playlist.PlaylistId,
                playlist.Name,
                0,
                0,
                0m,
                0);

            logger.LogInformation("[GetPlaylistEngagementQueryHandler.Handle] - Playlist has no tracks for PlaylistId: {PlaylistId}", request.PlaylistId);
            return Result.Ok(emptyDto);
        }

        var invoiceLineMetrics = await dbContext.InvoiceLines
            .AsNoTracking()
            .Where(il => playlistTrackIds.Contains(il.TrackId))
            .Join(dbContext.Invoices.AsNoTracking(), il => il.InvoiceId, i => i.InvoiceId, (il, i) => new
            {
                il.Quantity,
                Revenue = il.UnitPrice * il.Quantity,
                i.CustomerId
            })
            .ToListAsync(cancellationToken);

        var dto = new PlaylistEngagementDto(
            playlist.PlaylistId,
            playlist.Name,
            playlistTrackIds.Count,
            invoiceLineMetrics.Sum(x => x.Quantity),
            invoiceLineMetrics.Sum(x => x.Revenue),
            invoiceLineMetrics.Select(x => x.CustomerId).Distinct().Count());

        logger.LogInformation("[GetPlaylistEngagementQueryHandler.Handle] - Successfully computed playlist engagement for PlaylistId: {PlaylistId}", request.PlaylistId);
        return Result.Ok(dto);
    }
}
