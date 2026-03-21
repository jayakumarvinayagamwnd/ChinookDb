namespace Chinook.API.Features.Analytics;

public sealed record TopTrackDto(
    int TrackId,
    string TrackName,
    int UnitsSold,
    decimal Revenue);
