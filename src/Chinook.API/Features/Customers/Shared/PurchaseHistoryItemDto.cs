namespace Chinook.API.Features.Customers;

public sealed record PurchaseHistoryItemDto(
    int InvoiceId,
    DateTime InvoiceDate,
    string? BillingAddress,
    string? BillingCity,
    string? BillingCountry,
    decimal Total,
    int LineCount);
