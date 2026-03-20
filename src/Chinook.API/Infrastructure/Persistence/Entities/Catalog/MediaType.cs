namespace Chinook.API.Infrastructure.Persistence.Entities.Catalog;

public class MediaType
{
    public int MediaTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
