using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record ListInvoicesQuery : IResultQuery<List<InvoiceDto>>, ICacheableQuery
{
    public string CacheKey => "billing:invoices:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed record ListInvoicesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListInvoicesQueryHandler> logger) : IResultQueryHandler<ListInvoicesQuery, List<InvoiceDto>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListInvoicesQueryHandler.Handle] - Handling ListInvoicesQuery");

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceDate)
            .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListInvoicesQueryHandler.Handle] - Successfully retrieved {InvoiceCount} invoices", invoices.Count);
        return Result.Ok(invoices);
    }
}
