using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record UpdateCustomerSupportRepRequest(int? SupportRepId);

public sealed record UpdateCustomerSupportRepCommand(int CustomerId, int? SupportRepId) : IResultCommand<CustomerDto>;

public sealed record UpdateCustomerSupportRepCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<UpdateCustomerSupportRepCommandHandler> logger) : IResultCommandHandler<UpdateCustomerSupportRepCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerSupportRepCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[UpdateCustomerSupportRepCommandHandler.Handle] - Handling UpdateCustomerSupportRepCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .Where(c => c.CustomerId == request.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[UpdateCustomerSupportRepCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        if (request.SupportRepId.HasValue)
        {
            var supportRepExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == request.SupportRepId.Value, cancellationToken);

            if (!supportRepExists)
            {
                logger.LogWarning("[UpdateCustomerSupportRepCommandHandler.Handle] - Employee not found for SupportRepId: {SupportRepId}", request.SupportRepId);
                return Result.Fail($"Employee with ID {request.SupportRepId} not found.");
            }
        }

        customer.SupportRepId = request.SupportRepId;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[UpdateCustomerSupportRepCommandHandler.Handle] - Successfully updated support rep for CustomerId: {CustomerId}", customer.CustomerId);
        return Result.Ok(mapper.Map<CustomerDto>(customer));
    }
}
