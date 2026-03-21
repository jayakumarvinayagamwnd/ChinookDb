namespace Chinook.API.Features.Analytics;

public sealed record TopArtistDto(
    int ArtistId,
    string ArtistName,
    int UnitsSold,
    decimal Revenue);
