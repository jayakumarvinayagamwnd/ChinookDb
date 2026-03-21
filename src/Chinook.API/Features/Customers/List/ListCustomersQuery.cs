using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record ListCustomersQuery : IResultQuery<List<CustomerDto>>, ICacheableQuery
{
    public string CacheKey => "customers:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListCustomersQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListCustomersQueryHandler> logger) : IResultQueryHandler<ListCustomersQuery, List<CustomerDto>>
{
    public async Task<Result<List<CustomerDto>>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListCustomersQueryHandler.Handle] - Handling ListCustomersQuery");

        var customers = await dbContext.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListCustomersQueryHandler.Handle] - Successfully retrieved {CustomerCount} customers", customers.Count);
        return Result.Ok(customers);
    }
}
