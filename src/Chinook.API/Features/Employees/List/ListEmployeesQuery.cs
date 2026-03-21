using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record ListEmployeesQuery : IResultQuery<List<EmployeeDto>>, ICacheableQuery
{
    public string CacheKey => "employees:all";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListEmployeesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListEmployeesQueryHandler> logger) : IResultQueryHandler<ListEmployeesQuery, List<EmployeeDto>>
{
    public async Task<Result<List<EmployeeDto>>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListEmployeesQueryHandler.Handle] - Handling ListEmployeesQuery");

        var employees = await dbContext.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ProjectTo<EmployeeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[ListEmployeesQueryHandler.Handle] - Successfully retrieved {EmployeeCount} employees", employees.Count);
        return Result.Ok(employees);
    }
}
