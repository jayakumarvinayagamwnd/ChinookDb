using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Queries;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record GetEmployeeByIdQuery(int EmployeeId) : IResultQuery<EmployeeDto>, ICacheableQuery
{
    public string CacheKey => $"employees:{EmployeeId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(10);
}

public sealed record GetEmployeeByIdQueryHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<GetEmployeeByIdQueryHandler> logger) : IResultQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[GetEmployeeByIdQueryHandler.Handle] - Handling GetEmployeeByIdQuery for EmployeeId: {EmployeeId}", request.EmployeeId);

        var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeId == request.EmployeeId)
            .ProjectTo<EmployeeDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            logger.LogWarning("[GetEmployeeByIdQueryHandler.Handle] - Employee not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            return Result.Fail($"Employee with ID {request.EmployeeId} not found.");
        }

        logger.LogInformation("[GetEmployeeByIdQueryHandler.Handle] - Successfully retrieved employee for EmployeeId: {EmployeeId}", request.EmployeeId);
        return Result.Ok(employee);
    }
}
