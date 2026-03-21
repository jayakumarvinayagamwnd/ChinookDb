namespace Chinook.API.Features.Analytics;

public sealed record RevenueByCountryDto(
    string Country,
    decimal Revenue,
    int InvoiceCount);
