namespace Chinook.API.Features.Analytics;

public sealed record PlaylistEngagementDto(
    int PlaylistId,
    string PlaylistName,
    int TrackCount,
    int UnitsSold,
    decimal Revenue,
    int UniqueCustomers);
