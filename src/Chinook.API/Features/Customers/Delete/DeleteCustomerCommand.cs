using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Customers;

public sealed record DeleteCustomerCommand(int CustomerId) : IResultCommand<bool>;

public sealed record DeleteCustomerCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeleteCustomerCommandHandler> logger) : IResultCommandHandler<DeleteCustomerCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeleteCustomerCommandHandler.Handle] - Handling DeleteCustomerCommand for CustomerId: {CustomerId}", request.CustomerId);

        var customer = await dbContext.Customers
            .Where(c => c.CustomerId == request.CustomerId)
            .SingleOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("[DeleteCustomerCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var hasInvoices = await dbContext.Invoices
            .AsNoTracking()
            .AnyAsync(i => i.CustomerId == request.CustomerId, cancellationToken);

        if (hasInvoices)
        {
            logger.LogWarning("[DeleteCustomerCommandHandler.Handle] - Cannot delete CustomerId: {CustomerId} because dependent invoices exist", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} cannot be deleted because they have related invoices.");
        }

        dbContext.Customers.Remove(customer);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "[DeleteCustomerCommandHandler.Handle] - Delete failed due to related records for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} cannot be deleted because related records exist.");
        }

        logger.LogInformation("[DeleteCustomerCommandHandler.Handle] - Successfully deleted customer with CustomerId: {CustomerId}", request.CustomerId);
        return Result.Ok(true);
    }
}
