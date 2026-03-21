using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetAlbumsByArtistIdQuery(int ArtistId) : IResultQuery<List<AlbumDto>>, ICacheableQuery
{
    public string CacheKey => $"catalog:artist:{ArtistId}:albums";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetAlbumsByArtistIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetAlbumsByArtistIdQueryHandler> logger) : IResultQueryHandler<GetAlbumsByArtistIdQuery, List<AlbumDto>>
{
    public async Task<Result<List<AlbumDto>>> Handle(GetAlbumsByArtistIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetAlbumsByArtistIdQueryHandler.Handle] - Handling GetAlbumsByArtistIdQuery for ArtistId: {ArtistId}", request.ArtistId);

        var artistExists = await dbContext.Artists
            .AsNoTracking()
            .AnyAsync(a => a.ArtistId == request.ArtistId, cancellationToken);

        if (!artistExists)
        {
            logger.LogWarning("[GetAlbumsByArtistIdQueryHandler.Handle] - Artist not found for ArtistId: {ArtistId}", request.ArtistId);
            return Result.Fail($"Artist with ID {request.ArtistId} not found.");
        }

        var albums = await dbContext.Albums
            .AsNoTracking()
            .Where(a => a.ArtistId == request.ArtistId)
            .ProjectTo<AlbumDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetAlbumsByArtistIdQueryHandler.Handle] - Retrieved {AlbumCount} albums for ArtistId: {ArtistId}", albums.Count, request.ArtistId);

        return Result.Ok(albums);
    }
}
