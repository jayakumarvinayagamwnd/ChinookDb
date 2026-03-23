using Chinook.API.Common.Results;
using Chinook.API.Common.Pagination;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Billing;

public static class BillingEndpointExtensions
{
    public static WebApplication MapBillingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/billing").WithTags("Billing");

        group.MapGet("/invoices", GetInvoicesAsync)
            .WithName("GetInvoices")
            .WithSummary("Retrieves a list of all invoices.")
            .WithDescription("Returns invoices ordered by date descending using offset pagination.")
            .Produces<OffsetPagedResponse<InvoiceDto>>(StatusCodes.Status200OK);

        group.MapGet("/invoices/{invoiceId:int}", GetInvoiceByIdAsync)
            .WithName("GetInvoiceById")
            .WithSummary("Retrieves a single invoice by id.")
            .WithDescription("Returns one invoice by invoice id.")
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/invoices", CreateInvoiceAsync)
            .WithName("CreateInvoice")
            .WithSummary("Creates a new invoice.")
            .WithDescription("Adds a new invoice for a customer and returns the created resource.")
            .Produces<InvoiceDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/invoices/{invoiceId:int}/lines", AddInvoiceLineAsync)
            .WithName("AddInvoiceLine")
            .WithSummary("Adds a line item to an invoice.")
            .WithDescription("Adds a track as a line item to the specified invoice and recalculates the total.")
            .Produces<InvoiceLineDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/invoices/{invoiceId:int}/lines/{lineId:int}", DeleteInvoiceLineAsync)
            .WithName("DeleteInvoiceLine")
            .WithSummary("Removes a line item from an invoice.")
            .WithDescription("Deletes the specified line item and recalculates the invoice total.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/invoices/{invoiceId:int}/finalize", FinalizeInvoiceAsync)
            .WithName("FinalizeInvoice")
            .WithSummary("Finalizes an invoice.")
            .WithDescription("Validates that the invoice has at least one line item and a positive total. Returns the finalized invoice.")
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/invoices/{invoiceId:int}/void", VoidInvoiceAsync)
            .WithName("VoidInvoice")
            .WithSummary("Voids (deletes) an invoice.")
            .WithDescription("Removes the invoice and all its line items.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/customers/{customerId:int}/invoices", GetInvoicesByCustomerIdAsync)
            .WithName("GetInvoicesByCustomerId")
            .WithSummary("Retrieves all invoices for a customer.")
            .WithDescription("Returns all invoices for the specified customer, ordered by date descending.")
            .Produces<List<InvoiceDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/checkout", CheckoutAsync)
            .WithName("Checkout")
            .WithSummary("Creates an invoice with line items in one operation.")
            .WithDescription("Creates an invoice for a customer with the specified tracks, computing the total atomically.")
            .Produces<InvoiceDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/invoices/{invoiceId:int}/totals", GetInvoiceTotalsAsync)
            .WithName("GetInvoiceTotals")
            .WithSummary("Retrieves calculated totals for an invoice.")
            .WithDescription("Returns line count, subtotal (from lines), and stored total for the specified invoice.")
            .Produces<InvoiceTotalsDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/revenue", GetRevenueAsync)
            .WithName("GetRevenue")
            .WithSummary("Retrieves revenue totals for a date range.")
            .WithDescription("Returns total revenue and invoice count for invoices within the specified date range.")
            .Produces<RevenueDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetInvoicesAsync(int? offset, int? limit, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.GetInvoicesAsync] - Handling GetInvoicesAsync with Offset: {Offset}, Limit: {Limit}", offset, limit);
        var result = await mediator.Send(new ListInvoicesQuery(offset ?? 0, limit ?? OffsetPaginationDefaults.DefaultLimit), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetInvoiceByIdAsync(int invoiceId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.GetInvoiceByIdAsync] - Handling GetInvoiceByIdAsync for InvoiceId: {InvoiceId}", invoiceId);
        var result = await mediator.Send(new GetInvoiceByIdQuery(invoiceId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateInvoiceAsync(CreateInvoiceCommand request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.CreateInvoiceAsync] - Handling CreateInvoiceAsync for CustomerId: {CustomerId}", request.CustomerId);
        var result = await mediator.Send(request, cancellationToken);
        return result.ToCreatedResult($"/api/v1/billing/invoices/{result.Value?.InvoiceId}");
    }

    private static async Task<IResult> AddInvoiceLineAsync(int invoiceId, AddInvoiceLineCommand request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.AddInvoiceLineAsync] - Handling AddInvoiceLineAsync for InvoiceId: {InvoiceId}", invoiceId);
        var command = request with { InvoiceId = invoiceId };
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/billing/invoices/{invoiceId}/lines/{result.Value?.InvoiceLineId}");
    }

    private static async Task<IResult> DeleteInvoiceLineAsync(int invoiceId, int lineId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.DeleteInvoiceLineAsync] - Handling DeleteInvoiceLineAsync for InvoiceId: {InvoiceId}, LineId: {LineId}", invoiceId, lineId);
        var result = await mediator.Send(new DeleteInvoiceLineCommand(invoiceId, lineId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> FinalizeInvoiceAsync(int invoiceId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.FinalizeInvoiceAsync] - Handling FinalizeInvoiceAsync for InvoiceId: {InvoiceId}", invoiceId);
        var result = await mediator.Send(new FinalizeInvoiceCommand(invoiceId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> VoidInvoiceAsync(int invoiceId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.VoidInvoiceAsync] - Handling VoidInvoiceAsync for InvoiceId: {InvoiceId}", invoiceId);
        var result = await mediator.Send(new VoidInvoiceCommand(invoiceId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> GetInvoicesByCustomerIdAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.GetInvoicesByCustomerIdAsync] - Handling GetInvoicesByCustomerIdAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new GetInvoicesByCustomerIdQuery(customerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CheckoutAsync(CheckoutRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.CheckoutAsync] - Handling CheckoutAsync for CustomerId: {CustomerId}", request.CustomerId);
        var command = new CheckoutCommand(
            request.CustomerId,
            request.InvoiceDate,
            request.BillingAddress,
            request.BillingCity,
            request.BillingState,
            request.BillingCountry,
            request.BillingPostalCode,
            request.Items);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/billing/invoices/{result.Value?.InvoiceId}");
    }

    private static async Task<IResult> GetInvoiceTotalsAsync(int invoiceId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.GetInvoiceTotalsAsync] - Handling GetInvoiceTotalsAsync for InvoiceId: {InvoiceId}", invoiceId);
        var result = await mediator.Send(new GetInvoiceTotalsQuery(invoiceId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetRevenueAsync(DateTime from, DateTime to, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[BillingEndpointExtensions.GetRevenueAsync] - Handling GetRevenueAsync from {From} to {To}", from, to);
        var result = await mediator.Send(new GetRevenueQuery(from, to), cancellationToken);
        return result.ToHttpResult();
    }
}
