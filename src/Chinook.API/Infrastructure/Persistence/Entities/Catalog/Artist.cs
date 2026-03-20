namespace Chinook.API.Infrastructure.Persistence.Entities.Catalog;

public class Artist
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Album> Albums { get; set; } = new List<Album>();
}
