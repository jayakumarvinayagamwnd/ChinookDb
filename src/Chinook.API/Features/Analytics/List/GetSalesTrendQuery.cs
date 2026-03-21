using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetSalesTrendQuery(string Interval) : IResultQuery<List<SalesTrendPointDto>>, ICacheableQuery
{
    public string CacheKey => $"analytics:sales-trend:{Interval.ToLowerInvariant()}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetSalesTrendQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetSalesTrendQueryHandler> logger) : IResultQueryHandler<GetSalesTrendQuery, List<SalesTrendPointDto>>
{
    public async Task<Result<List<SalesTrendPointDto>>> Handle(GetSalesTrendQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetSalesTrendQueryHandler.Handle] - Handling GetSalesTrendQuery for Interval: {Interval}", request.Interval);

        var normalizedInterval = request.Interval.Trim().ToLowerInvariant();

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Select(i => new { i.InvoiceId, i.InvoiceDate, i.Total })
            .ToListAsync(cancellationToken);

        var lineUnits = await dbContext.InvoiceLines
            .AsNoTracking()
            .GroupBy(l => l.InvoiceId)
            .Select(g => new { InvoiceId = g.Key, Units = g.Sum(x => x.Quantity) })
            .ToListAsync(cancellationToken);

        var unitsByInvoice = lineUnits.ToDictionary(x => x.InvoiceId, x => x.Units);

        static DateTime ToPeriodStart(DateTime date, string interval)
        {
            return interval switch
            {
                "day" => new DateTime(date.Year, date.Month, date.Day),
                "month" => new DateTime(date.Year, date.Month, 1),
                "year" => new DateTime(date.Year, 1, 1),
                _ => new DateTime(date.Year, date.Month, 1)
            };
        }

        var points = invoices
            .GroupBy(i => ToPeriodStart(i.InvoiceDate, normalizedInterval))
            .OrderBy(g => g.Key)
            .Select(g => new SalesTrendPointDto(
                g.Key,
                g.Sum(x => x.Total),
                g.Count(),
                g.Sum(x => unitsByInvoice.TryGetValue(x.InvoiceId, out var units) ? units : 0)))
            .ToList();

        logger.LogInformation("[GetSalesTrendQueryHandler.Handle] - Successfully computed {PointCount} trend points", points.Count);
        return Result.Ok(points);
    }
}
