namespace Chinook.API.Features.Billing;

public sealed record InvoiceDto(
    int InvoiceId,
    int CustomerId,
    DateTime InvoiceDate,
    string? BillingAddress,
    string? BillingCity,
    string? BillingState,
    string? BillingCountry,
    string? BillingPostalCode,
    decimal Total);
