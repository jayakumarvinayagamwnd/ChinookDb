namespace Chinook.API.Features.Analytics;

public sealed record CohortRetentionDto(
    DateTime CohortMonth,
    DateTime ActivityMonth,
    int MonthsSinceCohort,
    int CohortSize,
    int ActiveCustomers,
    decimal RetentionRate);
