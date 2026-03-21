using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Customers;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string? Company,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Phone,
    string? Fax,
    string Email,
    int? SupportRepId) : IResultCommand<CustomerDto>;

public sealed record CreateCustomerCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CreateCustomerCommandHandler> logger) : IResultCommandHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CreateCustomerCommandHandler.Handle] - Handling CreateCustomerCommand for Email: {Email}", request.Email);

        if (request.SupportRepId.HasValue)
        {
            var supportRepExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.SupportRepId.Value, cancellationToken);

            if (!supportRepExists)
            {
                logger.LogWarning("[CreateCustomerCommandHandler.Handle] - Employee not found for SupportRepId: {SupportRepId}", request.SupportRepId);
                return Result.Fail($"Employee with ID {request.SupportRepId} not found.");
            }
        }

        var customer = mapper.Map<Customer>(request);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CreateCustomerCommandHandler.Handle] - Successfully created customer with CustomerId: {CustomerId}", customer.CustomerId);
        return Result.Ok(mapper.Map<CustomerDto>(customer));
    }
}
