using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record UpdateCustomerContactPreferencesRequest(
    string? Phone,
    string? Fax,
    string Email);

public sealed record UpdateCustomerContactPreferencesCommand(
    int CustomerId,
    string? Phone,
    string? Fax,
    string Email) : IResultCommand<CustomerDto>;

public sealed record UpdateCustomerContactPreferencesCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateCustomerContactPreferencesCommandHandler> logger) : IResultCommandHandler<UpdateCustomerContactPreferencesCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerContactPreferencesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateCustomerContactPreferencesCommandHandler.Handle] - Handling UpdateCustomerContactPreferencesCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .Where(c => c.CustomerId == request.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[UpdateCustomerContactPreferencesCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        customer.Phone = request.Phone;
        customer.Fax = request.Fax;
        customer.Email = request.Email;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateCustomerContactPreferencesCommandHandler.Handle] - Successfully updated contact preferences for CustomerId: {CustomerId}", customer.CustomerId);
        return Result.Ok(mapper.Map<CustomerDto>(customer));
    }
}
