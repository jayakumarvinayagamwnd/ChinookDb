using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record ListInvoicesQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<InvoiceDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"billing:invoices:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}

public sealed record ListInvoicesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListInvoicesQueryHandler> logger) : IResultQueryHandler<ListInvoicesQuery, OffsetPagedResponse<InvoiceDto>>
{
    public async Task<Result<OffsetPagedResponse<InvoiceDto>>> Handle(ListInvoicesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListInvoicesQueryHandler.Handle] - Handling ListInvoicesQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.InvoiceId)
            .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListInvoicesQueryHandler.Handle] - Successfully retrieved {InvoiceCount} invoices out of {TotalCount}", invoices.Items.Count, invoices.TotalCount);
        return Result.Ok(invoices);
    }
}
