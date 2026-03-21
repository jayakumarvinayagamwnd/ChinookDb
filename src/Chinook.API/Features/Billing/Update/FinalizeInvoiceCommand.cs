using AutoMapper;
using AutoMapper.QueryableExtensions;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

/// <summary>
/// Finalizes an invoice by validating it has at least one line and a positive total.
/// Note: The Invoice entity has no Status field, so this is a validation-acknowledgment action.
/// To persist finalized state, add a Status column to the invoices table.
/// </summary>
public sealed record FinalizeInvoiceCommand(int InvoiceId) : IResultCommand<InvoiceDto>;

public sealed class FinalizeInvoiceCommandValidator : AbstractValidator<FinalizeInvoiceCommand>
{
    public FinalizeInvoiceCommandValidator()
    {
        RuleFor(c => c.InvoiceId).GreaterThan(0);
    }
}

public sealed record FinalizeInvoiceCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<FinalizeInvoiceCommandHandler> logger) : IResultCommandHandler<FinalizeInvoiceCommand, InvoiceDto>
{
    public async Task<Result<InvoiceDto>> Handle(FinalizeInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[FinalizeInvoiceCommandHandler.Handle] - Handling FinalizeInvoiceCommand for InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await dbContext.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.InvoiceId == request.InvoiceId)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("[FinalizeInvoiceCommandHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        if (invoice.InvoiceLines.Count == 0)
        {
            logger.LogWarning("[FinalizeInvoiceCommandHandler.Handle] - Cannot finalize invoice with no lines for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} cannot be finalized because it has no line items.");
        }

        if (invoice.Total <= 0)
        {
            logger.LogWarning("[FinalizeInvoiceCommandHandler.Handle] - Cannot finalize invoice with zero total for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} cannot be finalized because the total is zero or negative.");
        }

        logger.LogInformation("[FinalizeInvoiceCommandHandler.Handle] - Invoice InvoiceId: {InvoiceId} passed finalization validation. Total: {Total}, Lines: {LineCount}", request.InvoiceId, invoice.Total, invoice.InvoiceLines.Count);
        return Result.Ok(mapper.Map<InvoiceDto>(invoice));
    }
}
