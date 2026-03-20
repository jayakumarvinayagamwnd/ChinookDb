using Chinook.API.Infrastructure.Persistence.Entities.Billing;
using Chinook.API.Infrastructure.Persistence.Entities.Catalog;
using Chinook.API.Infrastructure.Persistence.Entities.Customers;
using Chinook.API.Infrastructure.Persistence.Entities.Employees;
using Chinook.API.Infrastructure.Persistence.Entities.Playlists;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Infrastructure.Persistence;

public class ChinookDbContext(DbContextOptions<ChinookDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();

    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Artist>().ToTable("artists").HasKey(x => x.ArtistId);
        modelBuilder.Entity<Album>().ToTable("albums").HasKey(x => x.AlbumId);
        modelBuilder.Entity<Track>().ToTable("tracks").HasKey(x => x.TrackId);
        modelBuilder.Entity<Genre>().ToTable("genres").HasKey(x => x.GenreId);
        modelBuilder.Entity<MediaType>().ToTable("media_types").HasKey(x => x.MediaTypeId);

        modelBuilder.Entity<Playlist>().ToTable("playlists").HasKey(x => x.PlaylistId);
        modelBuilder.Entity<PlaylistTrack>().ToTable("playlist_track").HasKey(x => new { x.PlaylistId, x.TrackId });

        modelBuilder.Entity<Customer>().ToTable("customers").HasKey(x => x.CustomerId);

        modelBuilder.Entity<Employee>().ToTable("employees").HasKey(x => x.EmployeeId);

        modelBuilder.Entity<Invoice>().ToTable("invoices").HasKey(x => x.InvoiceId);
        modelBuilder.Entity<InvoiceLine>().ToTable("invoice_items").HasKey(x => x.InvoiceLineId);
    }
}
