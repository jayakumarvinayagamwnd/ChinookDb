using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Analytics;

public sealed record GetCustomerLtvQuery(int CustomerId) : IResultQuery<CustomerLtvDto>, ICacheableQuery
{
    public string CacheKey => $"analytics:customer-ltv:{CustomerId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetCustomerLtvQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetCustomerLtvQueryHandler> logger) : IResultQueryHandler<GetCustomerLtvQuery, CustomerLtvDto>
{
    public async Task<Result<CustomerLtvDto>> Handle(GetCustomerLtvQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetCustomerLtvQueryHandler.Handle] - Handling GetCustomerLtvQuery for CustomerId: {CustomerId}", request.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[GetCustomerLtvQueryHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == request.CustomerId)
            .Select(i => new { i.Total, i.InvoiceDate })
            .ToListAsync(cancellationToken);

        var lifetimeValue = invoices.Sum(i => i.Total);
        var invoiceCount = invoices.Count;
        var averageOrderValue = invoiceCount == 0 ? 0m : lifetimeValue / invoiceCount;
        DateTime? firstPurchaseDate = invoices.Count == 0 ? null : invoices.Min(i => i.InvoiceDate);
        DateTime? lastPurchaseDate = invoices.Count == 0 ? null : invoices.Max(i => i.InvoiceDate);

        var dto = new CustomerLtvDto(
            request.CustomerId,
            lifetimeValue,
            invoiceCount,
            averageOrderValue,
            firstPurchaseDate,
            lastPurchaseDate);

        logger.LogInformation("[GetCustomerLtvQueryHandler.Handle] - Successfully computed LTV for CustomerId: {CustomerId}", request.CustomerId);
        return Result.Ok(dto);
    }
}
