using Chinook.API.Infrastructure.Persistence.Entities.Catalog;

namespace Chinook.API.Infrastructure.Persistence.Entities.Playlists;

public class Playlist
{
    public int PlaylistId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
