using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record ListCustomersQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<CustomerDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"customers:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListCustomersQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListCustomersQueryHandler> logger) : IResultQueryHandler<ListCustomersQuery, OffsetPagedResponse<CustomerDto>>
{
    public async Task<Result<OffsetPagedResponse<CustomerDto>>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListCustomersQueryHandler.Handle] - Handling ListCustomersQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var customers = await dbContext.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ThenBy(c => c.CustomerId)
            .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListCustomersQueryHandler.Handle] - Successfully retrieved {CustomerCount} customers out of {TotalCount}", customers.Items.Count, customers.TotalCount);
        return Result.Ok(customers);
    }
}
