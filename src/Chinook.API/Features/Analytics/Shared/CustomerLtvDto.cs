namespace Chinook.API.Features.Analytics;

public sealed record CustomerLtvDto(
    int CustomerId,
    decimal LifetimeValue,
    int InvoiceCount,
    decimal AverageOrderValue,
    DateTime? FirstPurchaseDate,
    DateTime? LastPurchaseDate);
