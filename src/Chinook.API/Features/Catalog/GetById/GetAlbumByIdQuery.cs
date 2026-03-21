using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record GetAlbumByIdQuery(int AlbumId) : IResultQuery<AlbumDto>, ICacheableQuery
{
    public string CacheKey => $"catalog:album:{AlbumId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetAlbumByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetAlbumByIdQueryHandler> logger) : IResultQueryHandler<GetAlbumByIdQuery, AlbumDto>
{
    public async Task<Result<AlbumDto>> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetAlbumByIdQueryHandler.Handle] - Handling GetAlbumByIdQuery for AlbumId: {AlbumId}", request.AlbumId);

        var album = await dbContext.Albums
            .AsNoTracking()
            .Where(a => a.AlbumId == request.AlbumId)
            .ProjectTo<AlbumDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            logger.LogWarning("[GetAlbumByIdQueryHandler.Handle] - Album not found for AlbumId: {AlbumId}", request.AlbumId);
            return Result.Fail($"Album with ID {request.AlbumId} not found.");
        }

        logger.LogInformation("[GetAlbumByIdQueryHandler.Handle] - Successfully retrieved album for AlbumId: {AlbumId}", request.AlbumId);
        return Result.Ok(album);
    }
}
