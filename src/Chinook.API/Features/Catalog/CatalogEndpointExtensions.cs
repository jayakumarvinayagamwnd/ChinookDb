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

        return app;
    }

    private static async Task<IResult> GetArtistsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistsAsync] - Handling GetArtistsAsync");
        var result = await mediator.Send(new ListArtistsQuery(), cancellationToken);
        Log.Information("[CatalogEndpointExtensions.GetArtistsAsync] - Successfully retrieved {ArtistCount} artists", result.Count);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetArtistByIdAsync(int artistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[CatalogEndpointExtensions.GetArtistByIdAsync] - Handling GetArtistByIdAsync for ArtistId: {ArtistId}", artistId);

        var result = await mediator.Send(new GetArtistByIdQuery(artistId), cancellationToken);
        if (result is null)
        {
            Log.Information("[CatalogEndpointExtensions.GetArtistByIdAsync] - Artist not found for ArtistId: {ArtistId}", artistId);
            return Results.NotFound();
        }

        Log.Information("[CatalogEndpointExtensions.GetArtistByIdAsync] - Successfully retrieved artist for ArtistId: {ArtistId}", artistId);
        return Results.Ok(result);
    }
}