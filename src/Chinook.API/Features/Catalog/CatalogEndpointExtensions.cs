using Chinook.API.Common.Results;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Catalog;
public static class CatalogEndpointExtensions
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/catalog").WithTags("Catalog");

        group.MapGet("/artists", GetArtistsAsync)
            .WithName("GetArtists")
            .WithSummary("Retrieves a list of artists.")
            .WithDescription("Returns a list of all artists in the catalog.")
            .Produces<List<ArtistDto>>(StatusCodes.Status200OK);

        group.MapGet("/artists/{artistId:int}", GetArtistByIdAsync)
            .WithName("GetArtistById")
            .WithSummary("Retrieves a single artist by id.")
            .WithDescription("Returns one artist from the catalog by artist id.")
            .Produces<ArtistDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/artists", CreateArtistAsync)
            .WithName("CreateArtist")
            .WithSummary("Creates a new artist.")
            .WithDescription("Adds a new artist to the catalog and returns the created resource.")
            .Produces<ArtistDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/artists/{artistId:int}", DeleteArtistAsync)
            .WithName("DeleteArtist")
            .WithSummary("Deletes an artist by id.")
            .WithDescription("Permanently removes an artist from the catalog.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/artists/{artistId:int}", UpdateArtistAsync)
            .WithName("UpdateArtist")
            .WithSummary("Updates an existing artist.")
            .WithDescription("Applies partial updates to an artist in the catalog.")
            .Produces<ArtistDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/artists/{artistId:int}/albums", GetAlbumsByArtistIdAsync)
            .WithName("GetAlbumsByArtistId")
            .WithSummary("Retrieves all albums for an artist.")
            .WithDescription("Returns the list of albums belonging to the specified artist.")
            .Produces<List<AlbumDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/albums", GetAlbumsAsync)
            .WithName("GetAlbums")
            .WithSummary("Retrieves a list of albums.")
            .WithDescription("Returns a list of all albums in the catalog.")
            .Produces<List<AlbumDto>>(StatusCodes.Status200OK);

        group.MapGet("/albums/{albumId:int}", GetAlbumByIdAsync)
            .WithName("GetAlbumById")
            .WithSummary("Retrieves a single album by id.")
            .WithDescription("Returns one album from the catalog by album id.")
            .Produces<AlbumDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/albums", CreateAlbumAsync)
            .WithName("CreateAlbum")
            .WithSummary("Creates a new album.")
            .WithDescription("Adds a new album to the catalog and returns the created resource.")
            .Produces<AlbumDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/albums/{albumId:int}", UpdateAlbumAsync)
            .WithName("UpdateAlbum")
            .WithSummary("Updates an existing album.")
            .WithDescription("Applies partial updates to an album in the catalog.")
            .Produces<AlbumDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/albums/{albumId:int}", DeleteAlbumAsync)
            .WithName("DeleteAlbum")
            .WithSummary("Deletes an album by id.")
            .WithDescription("Permanently removes an album from the catalog.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/albums/{albumId:int}/tracks", GetTracksByAlbumIdAsync)
            .WithName("GetTracksByAlbumId")
            .WithSummary("Retrieves all tracks for an album.")
            .WithDescription("Returns the list of tracks belonging to the specified album.")
            .Produces<List<TrackDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/tracks", GetTracksAsync)
            .WithName("GetTracks")
            .WithSummary("Retrieves a list of tracks.")
            .WithDescription("Returns a list of all tracks in the catalog.")
            .Produces<List<TrackDto>>(StatusCodes.Status200OK);

        group.MapGet("/tracks/{trackId:int}", GetTrackByIdAsync)
            .WithName("GetTrackById")
            .WithSummary("Retrieves a single track by id.")
            .WithDescription("Returns one track from the catalog by track id.")
            .Produces<TrackDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/tracks", CreateTrackAsync)
            .WithName("CreateTrack")
            .WithSummary("Creates a new track.")
            .WithDescription("Adds a new track to the catalog and returns the created resource.")
            .Produces<TrackDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/tracks/{trackId:int}", UpdateTrackAsync)
            .WithName("UpdateTrack")
            .WithSummary("Updates an existing track.")
            .WithDescription("Applies partial updates to a track in the catalog.")
            .Produces<TrackDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/tracks/{trackId:int}", DeleteTrackAsync)
            .WithName("DeleteTrack")
            .WithSummary("Deletes a track by id.")
            .WithDescription("Permanently removes a track from the catalog.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/genres", GetGenresAsync)
            .WithName("GetGenres")
            .WithSummary("Retrieves a list of genres.")
            .WithDescription("Returns a list of all genres in the catalog.")
            .Produces<List<GenreDto>>(StatusCodes.Status200OK);

        group.MapGet("/media-types", GetMediaTypesAsync)
            .WithName("GetMediaTypes")
            .WithSummary("Retrieves a list of media types.")
            .WithDescription("Returns a list of all media types in the catalog.")
            .Produces<List<MediaTypeDto>>(StatusCodes.Status200OK);

        group.MapPost("/albums/{albumId:int}/publish", PublishAlbumAsync)
            .WithName("PublishAlbum")
            .WithSummary("Publishes an album.")
            .WithDescription("Validates and publishes the specified album.")
            .Produces<AlbumDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/tracks/{trackId:int}/reclassify-genre", ReclassifyTrackGenreAsync)
            .WithName("ReclassifyTrackGenre")
            .WithSummary("Reclassifies a track genre.")
            .WithDescription("Changes the genre assignment for the specified track.")
            .Produces<TrackDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/search", SearchCatalogAsync)
            .WithName("SearchCatalog")
            .WithSummary("Searches artists, albums, and tracks.")
            .WithDescription("Searches catalog resources by term and optional type filter.")
            .Produces<List<SearchResultDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetAlbumsByArtistIdAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetAlbumsByArtistIdAsync] - Handling GetAlbumsByArtistIdAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new GetAlbumsByArtistIdQuery(artistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTracksByAlbumIdAsync(int albumId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetTracksByAlbumIdAsync] - Handling GetTracksByAlbumIdAsync for AlbumId: {AlbumId}", albumId);
        var result = await mediator.Send(new GetTracksByAlbumIdQuery(albumId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetArtistsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistsAsync] - Handling GetArtistsAsync");
        var result = await mediator.Send(new ListArtistsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAlbumsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetAlbumsAsync] - Handling GetAlbumsAsync");
        var result = await mediator.Send(new ListAlbumsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTracksAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetTracksAsync] - Handling GetTracksAsync");
        var result = await mediator.Send(new ListTracksQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetGenresAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetGenresAsync] - Handling GetGenresAsync");
        var result = await mediator.Send(new ListGenresQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMediaTypesAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetMediaTypesAsync] - Handling GetMediaTypesAsync");
        var result = await mediator.Send(new ListMediaTypesQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SearchCatalogAsync(string q, string? type, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.SearchCatalogAsync] - Handling SearchCatalogAsync for Query: {Query}, Type: {Type}", q, type);
        var result = await mediator.Send(new SearchCatalogQuery(q, type), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateArtistAsync(CreateArtistCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.CreateArtistAsync] - Handling CreateArtistAsync for Name: {Name}", command.Name);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/catalog/artists/{result.Value?.ArtistId}");
    }

    private static async Task<IResult> CreateAlbumAsync(CreateAlbumCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.CreateAlbumAsync] - Handling CreateAlbumAsync for Title: {Title}", command.Title);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/catalog/albums/{result.Value?.AlbumId}");
    }

    private static async Task<IResult> CreateTrackAsync(CreateTrackCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.CreateTrackAsync] - Handling CreateTrackAsync for Name: {Name}", command.Name);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/catalog/tracks/{result.Value?.TrackId}");
    }

    private static async Task<IResult> PublishAlbumAsync(int albumId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.PublishAlbumAsync] - Handling PublishAlbumAsync for AlbumId: {AlbumId}", albumId);
        var result = await mediator.Send(new PublishAlbumCommand(albumId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ReclassifyTrackGenreAsync(int trackId, ReclassifyTrackGenreRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.ReclassifyTrackGenreAsync] - Handling ReclassifyTrackGenreAsync for TrackId: {TrackId}", trackId);
        var result = await mediator.Send(new ReclassifyTrackGenreCommand(trackId, request.GenreId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateArtistAsync(int artistId, UpdateArtistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.UpdateArtistAsync] - Handling UpdateArtistAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new UpdateArtistCommand(artistId, request.Name), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateAlbumAsync(int albumId, UpdateAlbumRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.UpdateAlbumAsync] - Handling UpdateAlbumAsync for AlbumId: {AlbumId}", albumId);
        var result = await mediator.Send(new UpdateAlbumCommand(albumId, request.Title, request.ArtistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateTrackAsync(int trackId, UpdateTrackRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.UpdateTrackAsync] - Handling UpdateTrackAsync for TrackId: {TrackId}", trackId);
        var result = await mediator.Send(new UpdateTrackCommand(
            trackId,
            request.Name,
            request.AlbumId,
            request.MediaTypeId,
            request.GenreId,
            request.Composer,
            request.Milliseconds,
            request.Bytes,
            request.UnitPrice), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteArtistAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.DeleteArtistAsync] - Handling DeleteArtistAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new DeleteArtistCommand(artistId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> DeleteAlbumAsync(int albumId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.DeleteAlbumAsync] - Handling DeleteAlbumAsync for AlbumId: {AlbumId}", albumId);
        var result = await mediator.Send(new DeleteAlbumCommand(albumId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> DeleteTrackAsync(int trackId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.DeleteTrackAsync] - Handling DeleteTrackAsync for TrackId: {TrackId}", trackId);
        var result = await mediator.Send(new DeleteTrackCommand(trackId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> GetArtistByIdAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistByIdAsync] - Handling GetArtistByIdAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new GetArtistByIdQuery(artistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAlbumByIdAsync(int albumId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetAlbumByIdAsync] - Handling GetAlbumByIdAsync for AlbumId: {AlbumId}", albumId);
        var result = await mediator.Send(new GetAlbumByIdQuery(albumId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTrackByIdAsync(int trackId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetTrackByIdAsync] - Handling GetTrackByIdAsync for TrackId: {TrackId}", trackId);
        var result = await mediator.Send(new GetTrackByIdQuery(trackId), cancellationToken);
        return result.ToHttpResult();
    }
}