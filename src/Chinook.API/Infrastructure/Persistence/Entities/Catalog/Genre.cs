namespace Chinook.API.Infrastructure.Persistence.Entities.Catalog;

public class Genre
{
    public int GenreId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
