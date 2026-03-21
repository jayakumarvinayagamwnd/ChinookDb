using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string? Title,
    int? ReportsTo,
    DateTime? BirthDate,
    DateTime? HireDate,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Phone,
    string? Fax,
    string? Email);

public sealed record UpdateEmployeeCommand(
    int EmployeeId,
    string FirstName,
    string LastName,
    string? Title,
    int? ReportsTo,
    DateTime? BirthDate,
    DateTime? HireDate,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Phone,
    string? Fax,
    string? Email) : IResultCommand<EmployeeDto>;

public sealed record UpdateEmployeeCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateEmployeeCommandHandler> logger) : IResultCommandHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateEmployeeCommandHandler.Handle] - Handling UpdateEmployeeCommand for EmployeeId: {EmployeeId}", request.EmployeeId);

        var employee = await dbContext.Employees
            .Where(e => e.EmployeeId == request.EmployeeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            logger.LogWarning("[UpdateEmployeeCommandHandler.Handle] - Employee not found for EmployeeId: {EmployeeId}", request.EmployeeId);
            return Result.Fail($"Employee with ID {request.EmployeeId} not found.");
        }

        if (request.ReportsTo.HasValue)
        {
            if (request.ReportsTo.Value == request.EmployeeId)
            {
                logger.LogWarning("[UpdateEmployeeCommandHandler.Handle] - Employee cannot report to themselves for EmployeeId: {EmployeeId}", request.EmployeeId);
                return Result.Fail("An employee cannot report to themselves.");
            }

            var managerExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.ReportsTo.Value, cancellationToken);

            if (!managerExists)
            {
                logger.LogWarning("[UpdateEmployeeCommandHandler.Handle] - Manager not found for ReportsTo: {ReportsTo}", request.ReportsTo);
                return Result.Fail($"Manager with ID {request.ReportsTo} not found.");
            }

            var visited = new HashSet<int>();
            var currentManagerId = request.ReportsTo;
            while (currentManagerId.HasValue)
            {
                if (!visited.Add(currentManagerId.Value))
                {
                    break;
                }

                if (currentManagerId.Value == request.EmployeeId)
                {
                    logger.LogWarning("[UpdateEmployeeCommandHandler.Handle] - Manager assignment would create a cycle for EmployeeId: {EmployeeId}", request.EmployeeId);
                    return Result.Fail("Manager assignment would create a reporting cycle.");
                }

                currentManagerId = await dbContext.Employees
                    .AsNoTracking()
                    .Where(e => e.EmployeeId == currentManagerId.Value)
                    .Select(e => e.ReportsTo)
                    .SingleOrDefaultAsync(cancellationToken);
            }
        }

        mapper.Map(request, employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateEmployeeCommandHandler.Handle] - Successfully updated employee with EmployeeId: {EmployeeId}", employee.EmployeeId);
        return Result.Ok(mapper.Map<EmployeeDto>(employee));
    }
}
