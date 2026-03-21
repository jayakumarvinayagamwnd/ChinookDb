using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record GetCustomerPurchaseHistoryQuery(int CustomerId) : IResultQuery<List<PurchaseHistoryItemDto>>, ICacheableQuery
{
    public string CacheKey => $"customers:{CustomerId}:purchase-history";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed record GetCustomerPurchaseHistoryQueryHandler(
    ChinookDbContext dbContext,
    ILogger<GetCustomerPurchaseHistoryQueryHandler> logger) : IResultQueryHandler<GetCustomerPurchaseHistoryQuery, List<PurchaseHistoryItemDto>>
{
    public async Task<Result<List<PurchaseHistoryItemDto>>> Handle(GetCustomerPurchaseHistoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetCustomerPurchaseHistoryQueryHandler.Handle] - Handling GetCustomerPurchaseHistoryQuery for CustomerId: {CustomerId}", request.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[GetCustomerPurchaseHistoryQueryHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var history = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == request.CustomerId)
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new PurchaseHistoryItemDto(
                i.InvoiceId,
                i.InvoiceDate,
                i.BillingAddress,
                i.BillingCity,
                i.BillingCountry,
                i.Total,
                i.InvoiceLines.Count))
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetCustomerPurchaseHistoryQueryHandler.Handle] - Retrieved {InvoiceCount} invoices for CustomerId: {CustomerId}", history.Count, request.CustomerId);
        return Result.Ok(history);
    }
}
