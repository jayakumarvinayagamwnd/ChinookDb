namespace Chinook.API.Features.Billing;

public sealed record InvoiceTotalsDto(
    int InvoiceId,
    int LineCount,
    decimal Subtotal,
    decimal Total);
