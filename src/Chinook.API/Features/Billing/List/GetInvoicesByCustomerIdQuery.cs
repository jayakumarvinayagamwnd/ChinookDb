using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record GetInvoicesByCustomerIdQuery(int CustomerId) : IResultQuery<List<InvoiceDto>>, ICacheableQuery
{
    public string CacheKey => $"billing:customer:{CustomerId}:invoices";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed class GetInvoicesByCustomerIdQueryValidator : AbstractValidator<GetInvoicesByCustomerIdQuery>
{
    public GetInvoicesByCustomerIdQueryValidator()
    {
        RuleFor(q => q.CustomerId).GreaterThan(0);
    }
}

public sealed record GetInvoicesByCustomerIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetInvoicesByCustomerIdQueryHandler> logger) : IResultQueryHandler<GetInvoicesByCustomerIdQuery, List<InvoiceDto>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(GetInvoicesByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetInvoicesByCustomerIdQueryHandler.Handle] - Handling GetInvoicesByCustomerIdQuery for CustomerId: {CustomerId}", request.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[GetInvoicesByCustomerIdQueryHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == request.CustomerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetInvoicesByCustomerIdQueryHandler.Handle] - Successfully retrieved {InvoiceCount} invoices for CustomerId: {CustomerId}", invoices.Count, request.CustomerId);
        return Result.Ok(invoices);
    }
}
