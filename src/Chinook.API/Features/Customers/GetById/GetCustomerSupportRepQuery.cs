using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record GetCustomerSupportRepQuery(int CustomerId) : IResultQuery<SupportRepDto>, ICacheableQuery
{
    public string CacheKey => $"customers:{CustomerId}:support-rep";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetCustomerSupportRepQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetCustomerSupportRepQueryHandler> logger) : IResultQueryHandler<GetCustomerSupportRepQuery, SupportRepDto>
{
    public async Task<Result<SupportRepDto>> Handle(GetCustomerSupportRepQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetCustomerSupportRepQueryHandler.Handle] - Handling GetCustomerSupportRepQuery for CustomerId: {CustomerId}", request.CustomerId);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[GetCustomerSupportRepQueryHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var supportRep = await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == request.CustomerId && c.SupportRepId.HasValue)
            .Select(c => c.SupportRep)
            .ProjectTo<SupportRepDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (supportRep is null)
        {
            logger.LogWarning("[GetCustomerSupportRepQueryHandler.Handle] - No support rep assigned for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} has no support rep assigned.");
        }

        logger.LogInformation("[GetCustomerSupportRepQueryHandler.Handle] - Successfully retrieved support rep for CustomerId: {CustomerId}", request.CustomerId);
        return Result.Ok(supportRep);
    }
}
