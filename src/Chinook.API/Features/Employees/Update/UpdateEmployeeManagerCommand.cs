using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record UpdateEmployeeManagerRequest(int? ManagerId);

public sealed record UpdateEmployeeManagerCommand(int EmployeeId, int? ManagerId) : IResultCommand<EmployeeDto>;

public sealed record UpdateEmployeeManagerCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateEmployeeManagerCommandHandler> logger) : IResultCommandHandler<UpdateEmployeeManagerCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeManagerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateEmployeeManagerCommandHandler.Handle] - Handling UpdateEmployeeManagerCommand for EmployeeId: {EmployeeId}", request.EmployeeId);

        var employee = await dbContext.Employees
            .Where(e => e.EmployeeId == request.EmployeeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            logger.LogWarning("[UpdateEmployeeManagerCommandHandler.Handle] - Employee not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            return Result.Fail($"Employee with ID {request.EmployeeId} not found.");
        }

        if (request.ManagerId.HasValue)
        {
            if (request.ManagerId.Value == request.EmployeeId)
            {
                logger.LogWarning("[UpdateEmployeeManagerCommandHandler.Handle] - Employee cannot be their own manager for EmployeeId: {EmployeeId}", request.EmployeeId);
                return Result.Fail("An employee cannot be their own manager.");
            }

            var managerExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.ManagerId.Value, cancellationToken);

            if (!managerExists)
            {
                logger.LogWarning("[UpdateEmployeeManagerCommandHandler.Handle] - Manager not found for ManagerId: {ManagerId}", request.ManagerId);
                return Result.Fail($"Manager with ID {request.ManagerId} not found.");
            }

            var visited = new HashSet<int>();
            var currentManagerId = request.ManagerId;
            while (currentManagerId.HasValue)
            {
                if (!visited.Add(currentManagerId.Value))
                {
                    break;
                }

                if (currentManagerId.Value == request.EmployeeId)
                {
                    logger.LogWarning("[UpdateEmployeeManagerCommandHandler.Handle] - Manager assignment would create a cycle for EmployeeId: {EmployeeId}", request.EmployeeId);
                    return Result.Fail("Manager assignment would create a reporting cycle.");
                }

                currentManagerId = await dbContext.Employees
                    .AsNoTracking()
                    .Where(e => e.EmployeeId == currentManagerId.Value)
                    .Select(e => e.ReportsTo)
                    .SingleOrDefaultAsync(cancellationToken);
            }
        }

        employee.ReportsTo = request.ManagerId;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateEmployeeManagerCommandHandler.Handle] - Successfully updated manager for EmployeeId: {EmployeeId}", request.EmployeeId);
        return Result.Ok(mapper.Map<EmployeeDto>(employee));
    }
}
