using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetRevenueByCountryQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<RevenueByCountryDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"analytics:revenue-by-country:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetRevenueByCountryQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetRevenueByCountryQueryHandler> logger) : IResultQueryHandler<GetRevenueByCountryQuery, OffsetPagedResponse<RevenueByCountryDto>>
{
    public async Task<Result<OffsetPagedResponse<RevenueByCountryDto>>> Handle(GetRevenueByCountryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetRevenueByCountryQueryHandler.Handle] - Handling GetRevenueByCountryQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var results = await dbContext.Invoices
            .AsNoTracking()
            .GroupBy(i => i.BillingCountry ?? "Unknown")
            .Select(g => new RevenueByCountryDto(
                g.Key,
                g.Sum(i => i.Total),
                g.Count()))
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.Country)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[GetRevenueByCountryQueryHandler.Handle] - Successfully retrieved {CountryCount} countries out of {TotalCount}", results.Items.Count, results.TotalCount);
        return Result.Ok(results);
    }
}
