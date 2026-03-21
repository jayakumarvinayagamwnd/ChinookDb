using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record GetEmployeeCustomersQuery(int EmployeeId) : IResultQuery<List<EmployeeCustomerDto>>, ICacheableQuery
{
    public string CacheKey => $"employees:{EmployeeId}:customers";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetEmployeeCustomersQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetEmployeeCustomersQueryHandler> logger) : IResultQueryHandler<GetEmployeeCustomersQuery, List<EmployeeCustomerDto>>
{
    public async Task<Result<List<EmployeeCustomerDto>>> Handle(GetEmployeeCustomersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetEmployeeCustomersQueryHandler.Handle] - Handling GetEmployeeCustomersQuery for EmployeeId: {EmployeeId}", request.EmployeeId);

        var employeeExists = await dbContext.Employees
            .AsNoTracking()
            .AnyAsync(e => e.EmployeeId == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            logger.LogWarning("[GetEmployeeCustomersQueryHandler.Handle] - Employee not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            return Result.Fail($"Employee with ID {request.EmployeeId} not found.");
        }

        var customers = await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.SupportRepId == request.EmployeeId)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ProjectTo<EmployeeCustomerDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetEmployeeCustomersQueryHandler.Handle] - Successfully retrieved {CustomerCount} customers for EmployeeId: {EmployeeId}", customers.Count, request.EmployeeId);
        return Result.Ok(customers);
    }
}
