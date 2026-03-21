namespace Chinook.API.Features.Billing;

public sealed record InvoiceLineDto(
    int InvoiceLineId,
    int InvoiceId,
    int TrackId,
    decimal UnitPrice,
    int Quantity);
