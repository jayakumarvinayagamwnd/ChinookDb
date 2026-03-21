namespace Chinook.API.Features.Analytics;

public sealed record SalesTrendPointDto(
    DateTime PeriodStart,
    decimal Revenue,
    int InvoiceCount,
    int UnitsSold);
