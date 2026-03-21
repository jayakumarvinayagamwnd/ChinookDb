using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record ReclassifyTrackGenreRequest(int GenreId);

public sealed record ReclassifyTrackGenreCommand(int TrackId, int GenreId) : IResultCommand<TrackDto>;

public sealed record ReclassifyTrackGenreCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ReclassifyTrackGenreCommandHandler> logger) : IResultCommandHandler<ReclassifyTrackGenreCommand, TrackDto>
{
    public async Task<Result<TrackDto>> Handle(ReclassifyTrackGenreCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ReclassifyTrackGenreCommandHandler.Handle] - Handling ReclassifyTrackGenreCommand for TrackId: {TrackId}", request.TrackId);

        var track = await dbContext.Tracks
            .Where(t => t.TrackId == request.TrackId)
            .SingleOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            logger.LogWarning("[ReclassifyTrackGenreCommandHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        var genreExists = await dbContext.Genres
            .AsNoTracking()
            .AnyAsync(g => g.GenreId == request.GenreId, cancellationToken);

        if (!genreExists)
        {
            logger.LogWarning("[ReclassifyTrackGenreCommandHandler.Handle] - Genre not found for GenreId: {GenreId}", request.GenreId);
            return Result.Fail($"Genre with ID {request.GenreId} not found.");
        }

        track.GenreId = request.GenreId;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[ReclassifyTrackGenreCommandHandler.Handle] - Successfully reclassified TrackId: {TrackId} to GenreId: {GenreId}", request.TrackId, request.GenreId);
        return Result.Ok(mapper.Map<TrackDto>(track));
    }
}
