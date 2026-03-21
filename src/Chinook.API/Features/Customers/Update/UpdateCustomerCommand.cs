using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record UpdateCustomerRequest(
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
    int? SupportRepId);

public sealed record UpdateCustomerCommand(
    int CustomerId,
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

public sealed record UpdateCustomerCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateCustomerCommandHandler> logger) : IResultCommandHandler<UpdateCustomerCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateCustomerCommandHandler.Handle] - Handling UpdateCustomerCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .Where(c => c.CustomerId == request.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[UpdateCustomerCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        if (request.SupportRepId.HasValue)
        {
            var supportRepExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.SupportRepId.Value, cancellationToken);

            if (!supportRepExists)
            {
                logger.LogWarning("[UpdateCustomerCommandHandler.Handle] - Employee not found for SupportRepId: {SupportRepId}", request.SupportRepId);
                return Result.Fail($"Employee with ID {request.SupportRepId} not found.");
            }
        }

        mapper.Map(request, customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateCustomerCommandHandler.Handle] - Successfully updated customer with CustomerId: {CustomerId}", customer.CustomerId);
        return Result.Ok(mapper.Map<CustomerDto>(customer));
    }
}
