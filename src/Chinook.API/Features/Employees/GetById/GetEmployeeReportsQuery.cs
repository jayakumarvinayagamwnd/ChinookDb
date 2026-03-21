using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record GetEmployeeReportsQuery(int EmployeeId) : IResultQuery<List<EmployeeDto>>, ICacheableQuery
{
    public string CacheKey => $"employees:{EmployeeId}:reports";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetEmployeeReportsQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetEmployeeReportsQueryHandler> logger) : IResultQueryHandler<GetEmployeeReportsQuery, List<EmployeeDto>>
{
    public async Task<Result<List<EmployeeDto>>> Handle(GetEmployeeReportsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetEmployeeReportsQueryHandler.Handle] - Handling GetEmployeeReportsQuery for EmployeeId: {EmployeeId}", request.EmployeeId);

        var employeeExists = await dbContext.Employees
            .AsNoTracking()
            .AnyAsync(e => e.EmployeeId == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            logger.LogWarning("[GetEmployeeReportsQueryHandler.Handle] - Employee not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            return Result.Fail($"Employee with ID {request.EmployeeId} not found.");
        }

        var reports = await dbContext.Employees
            .AsNoTracking()
            .Where(e => e.ReportsTo == request.EmployeeId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ProjectTo<EmployeeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        logger.LogInformation("[GetEmployeeReportsQueryHandler.Handle] - Successfully retrieved {ReportCount} reports for EmployeeId: {EmployeeId}", reports.Count, request.EmployeeId);
        return Result.Ok(reports);
    }
}
