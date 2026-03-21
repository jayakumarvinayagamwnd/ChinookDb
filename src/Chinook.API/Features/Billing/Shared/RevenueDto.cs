namespace Chinook.API.Features.Billing;

public sealed record RevenueDto(
    DateTime From,
    DateTime To,
    decimal TotalRevenue,
    int InvoiceCount);
