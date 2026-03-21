using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record DeleteInvoiceLineCommand(int InvoiceId, int InvoiceLineId) : IResultCommand<bool>;

public sealed class DeleteInvoiceLineCommandValidator : AbstractValidator<DeleteInvoiceLineCommand>
{
    public DeleteInvoiceLineCommandValidator()
    {
        RuleFor(c => c.InvoiceId).GreaterThan(0);
        RuleFor(c => c.InvoiceLineId).GreaterThan(0);
    }
}

public sealed record DeleteInvoiceLineCommandHandler(
    ChinookDbContext dbContext,
    ILogger<DeleteInvoiceLineCommandHandler> logger) : IResultCommandHandler<DeleteInvoiceLineCommand, bool>
{
    public async Task<Result<bool>> Handle(DeleteInvoiceLineCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DeleteInvoiceLineCommandHandler.Handle] - Handling DeleteInvoiceLineCommand for InvoiceId: {InvoiceId}, InvoiceLineId: {InvoiceLineId}", request.InvoiceId, request.InvoiceLineId);

        var invoice = await dbContext.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.InvoiceId == request.InvoiceId)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("[DeleteInvoiceLineCommandHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        var line = invoice.InvoiceLines.SingleOrDefault(l => l.InvoiceLineId == request.InvoiceLineId);

        if (line is null)
        {
            logger.LogWarning("[DeleteInvoiceLineCommandHandler.Handle] - InvoiceLine not found for InvoiceLineId: {InvoiceLineId}", request.InvoiceLineId);
            return Result.Fail($"Invoice line with ID {request.InvoiceLineId} not found on invoice {request.InvoiceId}.");
        }

        dbContext.InvoiceLines.Remove(line);
        invoice.Total = invoice.InvoiceLines
            .Where(l => l.InvoiceLineId != request.InvoiceLineId)
            .Sum(l => l.UnitPrice * l.Quantity);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[DeleteInvoiceLineCommandHandler.Handle] - Successfully removed InvoiceLineId: {InvoiceLineId} from InvoiceId: {InvoiceId}", request.InvoiceLineId, request.InvoiceId);
        return Result.Ok(true);
    }
}
