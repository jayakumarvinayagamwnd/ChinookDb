using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Common.Pagination;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record ListEmployeesQuery(
    int Offset = 0,
    int Limit = OffsetPaginationDefaults.DefaultLimit) : IResultQuery<OffsetPagedResponse<EmployeeDto>>, ICacheableQuery, IOffsetPaginatedQuery
{
    public string CacheKey => $"employees:{Offset}:{Limit}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record ListEmployeesQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<ListEmployeesQueryHandler> logger) : IResultQueryHandler<ListEmployeesQuery, OffsetPagedResponse<EmployeeDto>>
{
    public async Task<Result<OffsetPagedResponse<EmployeeDto>>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ListEmployeesQueryHandler.Handle] - Handling ListEmployeesQuery with Offset: {Offset}, Limit: {Limit}", request.Offset, request.Limit);

        var employees = await dbContext.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ThenBy(e => e.EmployeeId)
            .ProjectTo<EmployeeDto>(mapper.ConfigurationProvider)
            .ToOffsetPagedResponseAsync(request.Offset, request.Limit, cancellationToken);

        logger.LogInformation("[ListEmployeesQueryHandler.Handle] - Successfully retrieved {EmployeeCount} employees out of {TotalCount}", employees.Items.Count, employees.TotalCount);
        return Result.Ok(employees);
    }
}
