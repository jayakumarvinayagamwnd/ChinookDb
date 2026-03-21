using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record GetCustomerByIdQuery(int CustomerId) : IResultQuery<CustomerDto>, ICacheableQuery
{
    public string CacheKey => $"customers:{CustomerId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetCustomerByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetCustomerByIdQueryHandler> logger) : IResultQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetCustomerByIdQueryHandler.Handle] - Handling GetCustomerByIdQuery for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == request.CustomerId)
            .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[GetCustomerByIdQueryHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        logger.LogInformation("[GetCustomerByIdQueryHandler.Handle] - Successfully retrieved customer for CustomerId: {CustomerId}", request.CustomerId);
        return Result.Ok(customer);
    }
}
