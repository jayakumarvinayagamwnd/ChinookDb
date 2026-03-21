using AutoMapper;
using Chinook.API.Common.Contracts.Commands;
using Chinook.API.Infrastructure.Persistence;
using Chinook.API.Infrastructure.Persistence.Entities.Billing;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Features.Billing;

public sealed record CheckoutLineItem(int TrackId, int Quantity);

public sealed record CheckoutRequest(
    int CustomerId,
    DateTime InvoiceDate,
    string? BillingAddress,
    string? BillingCity,
    string? BillingState,
    string? BillingCountry,
    string? BillingPostalCode,
    List<CheckoutLineItem> Items);

public sealed record CheckoutCommand(
    int CustomerId,
    DateTime InvoiceDate,
    string? BillingAddress,
    string? BillingCity,
    string? BillingState,
    string? BillingCountry,
    string? BillingPostalCode,
    List<CheckoutLineItem> Items) : IResultCommand<InvoiceDto>;

public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(c => c.CustomerId).GreaterThan(0);
        RuleFor(c => c.InvoiceDate).NotEmpty();
        RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.TrackId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public sealed record CheckoutCommandHandler(
    ChinookDbContext dbContext,
    IMapper mapper,
    ILogger<CheckoutCommandHandler> logger) : IResultCommandHandler<CheckoutCommand, InvoiceDto>
{
    public async Task<Result<InvoiceDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("[CheckoutCommandHandler.Handle] - Handling CheckoutCommand for CustomerId: {CustomerId} with {ItemCount} items", request.CustomerId, request.Items.Count);

        var customerExists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            logger.LogWarning("[CheckoutCommandHandler.Handle] - Customer not found for CustomerId: {CustomerId}", request.CustomerId);
            return Result.Fail($"Customer with ID {request.CustomerId} not found.");
        }

        var requestedTrackIds = request.Items.Select(i => i.TrackId).Distinct().ToList();
        var tracks = await dbContext.Tracks
            .AsNoTracking()
            .Where(t => requestedTrackIds.Contains(t.TrackId))
            .Select(t => new { t.TrackId, t.UnitPrice })
            .ToListAsync(cancellationToken);

        var missingTrackIds = requestedTrackIds.Except(tracks.Select(t => t.TrackId)).ToList();
        if (missingTrackIds.Count > 0)
        {
            logger.LogWarning("[CheckoutCommandHandler.Handle] - Tracks not found: {MissingTrackIds}", missingTrackIds);
            return Result.Fail($"Tracks with IDs [{string.Join(", ", missingTrackIds)}] not found.");
        }

        var trackPriceMap = tracks.ToDictionary(t => t.TrackId, t => t.UnitPrice);

        var invoice = new Invoice
        {
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate,
            BillingAddress = request.BillingAddress,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingCountry = request.BillingCountry,
            BillingPostalCode = request.BillingPostalCode,
            Total = 0m
        };

        dbContext.Invoices.Add(invoice);

        var lines = request.Items.Select(item => new InvoiceLine
        {
            Invoice = invoice,
            TrackId = item.TrackId,
            UnitPrice = trackPriceMap[item.TrackId],
            Quantity = item.Quantity
        }).ToList();

        dbContext.InvoiceLines.AddRange(lines);
        invoice.Total = lines.Sum(l => l.UnitPrice * l.Quantity);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CheckoutCommandHandler.Handle] - Successfully created invoice InvoiceId: {InvoiceId} with Total: {Total}", invoice.InvoiceId, invoice.Total);
        return Result.Ok(mapper.Map<InvoiceDto>(invoice));
    }
}
