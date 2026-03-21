namespace Chinook.API.Features.Catalog;

public sealed record TrackDto(
    int TrackId,
    string Name,
    int? AlbumId,
    int MediaTypeId,
    int? GenreId,
    string? Composer,
    int Milliseconds,
    int? Bytes,
    decimal UnitPrice);
