using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetCohortRetentionQuery : IResultQuery<List<CohortRetentionDto>>, ICacheableQuery
{
    public string CacheKey => "analytics:cohort-retention";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public sealed record GetCohortRetentionQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetCohortRetentionQueryHandler> logger) : IResultQueryHandler<GetCohortRetentionQuery, List<CohortRetentionDto>>
{
    public async Task<Result<List<CohortRetentionDto>>> Handle(GetCohortRetentionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetCohortRetentionQueryHandler.Handle] - Handling GetCohortRetentionQuery");

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Select(i => new { i.CustomerId, i.InvoiceDate })
            .ToListAsync(cancellationToken);

        if (invoices.Count == 0)
        {
            logger.LogInformation("[GetCohortRetentionQueryHandler.Handle] - No invoices found. Returning empty cohort retention data.");
            return Result.Ok(new List<CohortRetentionDto>());
        }

        static DateTime MonthStart(DateTime date) => new(date.Year, date.Month, 1);

        var firstPurchaseByCustomer = invoices
            .GroupBy(i => i.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => MonthStart(g.Min(x => x.InvoiceDate)));

        var activeMonthsByCustomer = invoices
            .GroupBy(i => i.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => MonthStart(x.InvoiceDate)).Distinct().ToHashSet());

        var cohortGroups = firstPurchaseByCustomer
            .GroupBy(kvp => kvp.Value)
            .OrderBy(g => g.Key)
            .ToList();

        var latestActivityMonth = MonthStart(invoices.Max(i => i.InvoiceDate));
        var results = new List<CohortRetentionDto>();

        foreach (var cohortGroup in cohortGroups)
        {
            var cohortMonth = cohortGroup.Key;
            var cohortCustomerIds = cohortGroup.Select(x => x.Key).ToList();
            var cohortSize = cohortCustomerIds.Count;

            for (var activityMonth = cohortMonth; activityMonth <= latestActivityMonth; activityMonth = activityMonth.AddMonths(1))
            {
                var activeCustomers = cohortCustomerIds.Count(customerId =>
                    activeMonthsByCustomer.TryGetValue(customerId, out var activeMonths) && activeMonths.Contains(activityMonth));

                var monthsSinceCohort = ((activityMonth.Year - cohortMonth.Year) * 12) + activityMonth.Month - cohortMonth.Month;
                var retentionRate = cohortSize == 0 ? 0m : Math.Round((decimal)activeCustomers / cohortSize, 4);

                results.Add(new CohortRetentionDto(
                    cohortMonth,
                    activityMonth,
                    monthsSinceCohort,
                    cohortSize,
                    activeCustomers,
                    retentionRate));
            }
        }

        logger.LogInformation("[GetCohortRetentionQueryHandler.Handle] - Successfully computed {PointCount} cohort retention points", results.Count);
        return Result.Ok(results);
    }
}
