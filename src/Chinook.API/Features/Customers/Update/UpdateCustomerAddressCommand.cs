using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record UpdateCustomerAddressRequest(
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode);

public sealed record UpdateCustomerAddressCommand(
    int CustomerId,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode) : IResultCommand<CustomerDto>;

public sealed record UpdateCustomerAddressCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateCustomerAddressCommandHandler> logger) : IResultCommandHandler<UpdateCustomerAddressCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateCustomerAddressCommandHandler.Handle] - Handling UpdateCustomerAddressCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .Where(c => c.CustomerId == request.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[UpdateCustomerAddressCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        customer.Address = request.Address;
        customer.City = request.City;
        customer.State = request.State;
        customer.Country = request.Country;
        customer.PostalCode = request.PostalCode;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateCustomerAddressCommandHandler.Handle] - Successfully updated address for CustomerId: {CustomerId}", customer.CustomerId);
        return Result.Ok(mapper.Map<CustomerDto>(customer));
    }
}
