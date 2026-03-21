using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Employees;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Employees;

public sealed record CreateEmployeeCommand(
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

public sealed record CreateEmployeeCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateEmployeeCommandHandler> logger) : IResultCommandHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateEmployeeCommandHandler.Handle] - Handling CreateEmployeeCommand for Email: {Email}", request.Email);

        if (request.ReportsTo.HasValue)
        {
            var managerExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.ReportsTo.Value, cancellationToken);

            if (!managerExists)
            {
                logger.LogWarning("[CreateEmployeeCommandHandler.Handle] - Manager not found for ReportsTo: {ReportsTo}", request.ReportsTo);
                return Result.Fail($"Manager with ID {request.ReportsTo} not found.");
            }
        }

        var employee = mapper.Map<Employee>(request);
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateEmployeeCommandHandler.Handle] - Successfully created employee with EmployeeId: {EmployeeId}", employee.EmployeeId);
        return Result.Ok(mapper.Map<EmployeeDto>(employee));
    }
}
