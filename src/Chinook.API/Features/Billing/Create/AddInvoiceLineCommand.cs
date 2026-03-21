using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Billing;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record AddInvoiceLineCommand(int InvoiceId, int TrackId, int Quantity) : IResultCommand<InvoiceLineDto>;

public sealed class AddInvoiceLineCommandValidator : AbstractValidator<AddInvoiceLineCommand>
{
    public AddInvoiceLineCommandValidator()
    {
        RuleFor(c => c.InvoiceId).GreaterThan(0);
        RuleFor(c => c.TrackId).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}

public sealed record AddInvoiceLineCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<AddInvoiceLineCommandHandler> logger) : IResultCommandHandler<AddInvoiceLineCommand, InvoiceLineDto>
{
    public async Task<Result<InvoiceLineDto>> Handle(AddInvoiceLineCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[AddInvoiceLineCommandHandler.Handle] - Handling AddInvoiceLineCommand for InvoiceId: {InvoiceId}, TrackId: {TrackId}", request.InvoiceId, request.TrackId);

        var invoice = await dbContext.Invoices
            .Include(i => i.InvoiceLines)
            .Where(i => i.InvoiceId == request.InvoiceId)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("[AddInvoiceLineCommandHandler.Handle] - Invoice not found for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Result.Fail($"Invoice with ID {request.InvoiceId} not found.");
        }

        var track = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => t.TrackId == request.TrackId)
            .Select(t => new { t.TrackId, t.UnitPrice })
            .SingleOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            logger.LogWarning("[AddInvoiceLineCommandHandler.Handle] - Track not found for TrackId: {TrackId}", request.TrackId);
            return Result.Fail($"Track with ID {request.TrackId} not found.");
        }

        var line = new InvoiceLine
        {
            InvoiceId = invoice.InvoiceId,
            TrackId = track.TrackId,
            UnitPrice = track.UnitPrice,
            Quantity = request.Quantity
        };

        dbContext.InvoiceLines.Add(line);
        invoice.Total = invoice.InvoiceLines.Sum(l => l.UnitPrice * l.Quantity) + (track.UnitPrice * request.Quantity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[AddInvoiceLineCommandHandler.Handle] - Successfully added line InvoiceLineId: {InvoiceLineId} to InvoiceId: {InvoiceId}", line.InvoiceLineId, invoice.InvoiceId);
        return Result.Ok(mapper.Map<InvoiceLineDto>(line));
    }
}
