using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

/// <summary>
/// Voids an invoice by removing it and all its line items.
/// Note: The Invoice entity has no Status field, so void is implemented as a deletion.
/// To preserve voided invoices, add a Status column to the invoices table.
/// </summary>
public sealed record VoidInvoiceCommand(int InvoiceId) : IResultCommand<bool>;

public sealed class VoidInvoiceCommandValidator : AbstractValidator<VoidInvoiceCommand>
{
    public VoidInvoiceCommandValidator()
    {
        RuleFor(c => c.InvoiceId).GreaterThan(0);
    }
}

public sealed record VoidInvoiceCommandHandler(
    ChinookDbContext dbContext,
    ILogger<VoidInvoiceCommandHandler> logger) : IResultCommandHandler<VoidInvoiceCommand, bool>
{
    public async Task<Result<bool>> Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[VoidInvoiceCommandHandler.Handle] - Handling VoidInvoiceCommand for InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await dbContext.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.InvoiceId == request.InvoiceId)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("[VoidInvoiceCommandHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        dbContext.InvoiceLines.RemoveRange(invoice.InvoiceLines);
        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[VoidInvoiceCommandHandler.Handle] - Successfully voided (deleted) InvoiceId: {InvoiceId}", request.InvoiceId);
        return Result.Ok(true);
    }
}
