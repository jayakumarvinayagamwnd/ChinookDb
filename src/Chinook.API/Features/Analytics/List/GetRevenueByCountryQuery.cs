using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetRevenueByCountryQuery : IResultQuery<List<RevenueByCountryDto>>, ICacheableQuery
{
    public string CacheKey => "analytics:revenue-by-country";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetRevenueByCountryQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetRevenueByCountryQueryHandler> logger) : IResultQueryHandler<GetRevenueByCountryQuery, List<RevenueByCountryDto>>
{
    public async Task<Result<List<RevenueByCountryDto>>> Handle(GetRevenueByCountryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetRevenueByCountryQueryHandler.Handle] - Handling GetRevenueByCountryQuery");

        var results = await dbContext.Invoices
            .AsNoTracking()
            .GroupBy(i => i.BillingCountry ?? "Unknown")
            .Select(g => new RevenueByCountryDto(
                g.Key,
                g.Sum(i => i.Total),
                g.Count()))
            .OrderByDescending(x => x.Revenue)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetRevenueByCountryQueryHandler.Handle] - Successfully retrieved {CountryCount} countries", results.Count);
        return Result.Ok(results);
    }
}
