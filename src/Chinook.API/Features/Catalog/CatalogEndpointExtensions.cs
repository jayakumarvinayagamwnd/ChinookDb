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

        return app;
    }

    private static async Task<IResult> GetAlbumsByArtistIdAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetAlbumsByArtistIdAsync] - Handling GetAlbumsByArtistIdAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new GetAlbumsByArtistIdQuery(artistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetArtistsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistsAsync] - Handling GetArtistsAsync");
        var result = await mediator.Send(new ListArtistsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateArtistAsync(CreateArtistCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.CreateArtistAsync] - Handling CreateArtistAsync for Name: {Name}", command.Name);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/catalog/artists/{result.Value?.ArtistId}");
    }

    private static async Task<IResult> UpdateArtistAsync(int artistId, UpdateArtistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.UpdateArtistAsync] - Handling UpdateArtistAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new UpdateArtistCommand(artistId, request.Name), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteArtistAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.DeleteArtistAsync] - Handling DeleteArtistAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new DeleteArtistCommand(artistId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> GetArtistByIdAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistByIdAsync] - Handling GetArtistByIdAsync for ArtistId: {ArtistId}", artistId);
        var result = await mediator.Send(new GetArtistByIdQuery(artistId), cancellationToken);
        return result.ToHttpResult();
    }
}