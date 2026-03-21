using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Catalog;

public sealed record SearchCatalogQuery(string SearchTerm, string? Type) : IResultQuery<List<SearchResultDto>>, ICacheableQuery
{
    public string CacheKey => $"catalog:search:{SearchTerm.Trim().ToLowerInvariant()}:{NormalizeType(Type)}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);

    private static string NormalizeType(string? type) => string.IsNullOrWhiteSpace(type)
        ? "artist,album,track"
        : string.Join(',', type.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .OrderBy(t => t));
}

public sealed record SearchCatalogQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<SearchCatalogQueryHandler> logger) : IResultQueryHandler<SearchCatalogQuery, List<SearchResultDto>>
{
    private static readonly HashSet<string> SupportedTypes = new(["artist", "album", "track"]);

    public async Task<Result<List<SearchResultDto>>> Handle(SearchCatalogQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[SearchCatalogQueryHandler.Handle] - Handling SearchCatalogQuery for SearchTerm: {SearchTerm}, Type: {Type}", request.SearchTerm, request.Type);

        var normalizedTypes = ParseTypes(request.Type);
        var normalizedSearchTerm = request.SearchTerm.Trim();
        var results = new List<SearchResultDto>();

        if (normalizedTypes.Contains("artist"))
        {
            var artists = await dbContext.Artists
                .AsNoTracking()
                .Where(a => EF.Functions.Like(a.Name, $"%{normalizedSearchTerm}%"))
                .ProjectTo<SearchResultDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            results.AddRange(artists);
        }

        if (normalizedTypes.Contains("album"))
        {
            var albums = await dbContext.Albums
                .AsNoTracking()
                .Where(a => EF.Functions.Like(a.Title, $"%{normalizedSearchTerm}%"))
                .ProjectTo<SearchResultDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            results.AddRange(albums);
        }

        if (normalizedTypes.Contains("track"))
        {
            var tracks = await dbContext.Tracks
                .AsNoTracking()
                .Where(t => EF.Functions.Like(t.Name, $"%{normalizedSearchTerm}%"))
                .ProjectTo<SearchResultDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            results.AddRange(tracks);
        }

        logger.LogInformation("[SearchCatalogQueryHandler.Handle] - Retrieved {ResultCount} search results", results.Count);
        return Result.Ok(results.OrderBy(r => r.Type).ThenBy(r => r.Name).ToList());
    }

    private static HashSet<string> ParseTypes(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return new HashSet<string>(SupportedTypes);

        return type.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(SupportedTypes.Contains)
            .ToHashSet();
    }
}
