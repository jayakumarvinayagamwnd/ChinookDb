using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record GetEmployeeHierarchyQuery : IResultQuery<List<EmployeeHierarchyNodeDto>>, ICacheableQuery
{
    public string CacheKey => "employees:hierarchy";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15);
}

public sealed record GetEmployeeHierarchyQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetEmployeeHierarchyQueryHandler> logger) : IResultQueryHandler<GetEmployeeHierarchyQuery, List<EmployeeHierarchyNodeDto>>
{
    public async Task<Result<List<EmployeeHierarchyNodeDto>>> Handle(GetEmployeeHierarchyQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetEmployeeHierarchyQueryHandler.Handle] - Handling GetEmployeeHierarchyQuery");

        var employees = await dbContext.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ProjectTo<EmployeeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var groupedByManager = employees
            .ToLookup(e => e.ReportsTo);

        List<EmployeeHierarchyNodeDto> BuildNodes(int? managerId, HashSet<int> path)
        {
            var reports = groupedByManager[managerId].ToList();
            if (reports.Count == 0)
            {
                return [];
            }

            return reports
                .Select(e => new EmployeeHierarchyNodeDto(
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.Title,
                    e.ReportsTo,
                    path.Contains(e.EmployeeId)
                        ? []
                        : BuildNodes(e.EmployeeId, new HashSet<int>(path) { e.EmployeeId })))
                .ToList();
        }

        var hierarchy = BuildNodes(null, new HashSet<int>());

        logger.LogInformation("[GetEmployeeHierarchyQueryHandler.Handle] - Successfully built hierarchy with {RootCount} root nodes", hierarchy.Count);
        return Result.Ok(hierarchy);
    }
}
