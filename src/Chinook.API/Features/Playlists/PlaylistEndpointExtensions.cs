using Chinook.API.Common.Results;
using Chinook.API.Features.Catalog;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Playlists;

public static class PlaylistEndpointExtensions
{
    public static WebApplication MapPlaylistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/playlists").WithTags("Playlists");

        group.MapGet("/", GetPlaylistsAsync)
            .WithName("GetPlaylists")
            .WithSummary("Retrieves a list of playlists.")
            .WithDescription("Returns a list of all playlists.")
            .Produces<List<PlaylistDto>>(StatusCodes.Status200OK);

        group.MapGet("/{playlistId:int}", GetPlaylistByIdAsync)
            .WithName("GetPlaylistById")
            .WithSummary("Retrieves a single playlist by id.")
            .WithDescription("Returns one playlist by playlist id.")
            .Produces<PlaylistDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePlaylistAsync)
            .WithName("CreatePlaylist")
            .WithSummary("Creates a new playlist.")
            .WithDescription("Adds a new playlist and returns the created resource.")
            .Produces<PlaylistDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{playlistId:int}", UpdatePlaylistAsync)
            .WithName("UpdatePlaylist")
            .WithSummary("Updates an existing playlist.")
            .WithDescription("Applies partial updates to a playlist.")
            .Produces<PlaylistDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{playlistId:int}", DeletePlaylistAsync)
            .WithName("DeletePlaylist")
            .WithSummary("Deletes a playlist by id.")
            .WithDescription("Permanently removes a playlist and all its track associations.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{playlistId:int}/tracks", GetTracksByPlaylistIdAsync)
            .WithName("GetTracksByPlaylistId")
            .WithSummary("Retrieves all tracks for a playlist.")
            .WithDescription("Returns the list of tracks belonging to the specified playlist.")
            .Produces<List<TrackDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{playlistId:int}/tracks", AddTrackToPlaylistAsync)
            .WithName("AddTrackToPlaylist")
            .WithSummary("Adds a track to a playlist.")
            .WithDescription("Associates an existing track with the specified playlist.")
            .Produces<PlaylistDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{playlistId:int}/tracks/{trackId:int}", RemoveTrackFromPlaylistAsync)
            .WithName("RemoveTrackFromPlaylist")
            .WithSummary("Removes a track from a playlist.")
            .WithDescription("Removes the association between a track and the specified playlist.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{playlistId:int}/reorder", ReorderPlaylistAsync)
            .WithName("ReorderPlaylist")
            .WithSummary("Reorders tracks in a playlist.")
            .WithDescription("Returns the playlist tracks in the requested order. Provide all current TrackIds in the desired sequence.")
            .Produces<List<TrackDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{playlistId:int}/clone", ClonePlaylistAsync)
            .WithName("ClonePlaylist")
            .WithSummary("Clones a playlist.")
            .WithDescription("Creates a new playlist as a copy of the specified playlist, including all its tracks.")
            .Produces<PlaylistDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{playlistId:int}/recommendations", GetPlaylistRecommendationsAsync)
            .WithName("GetPlaylistRecommendations")
            .WithSummary("Gets track recommendations for a playlist.")
            .WithDescription("Returns tracks from matching genres that are not already in the playlist.")
            .Produces<List<TrackDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetPlaylistsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.GetPlaylistsAsync] - Handling GetPlaylistsAsync");
        var result = await mediator.Send(new ListPlaylistsQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPlaylistByIdAsync(int playlistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.GetPlaylistByIdAsync] - Handling GetPlaylistByIdAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new GetPlaylistByIdQuery(playlistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreatePlaylistAsync(CreatePlaylistCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.CreatePlaylistAsync] - Handling CreatePlaylistAsync for Name: {Name}", command.Name);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedResult($"/api/v1/playlists/{result.Value?.PlaylistId}");
    }

    private static async Task<IResult> UpdatePlaylistAsync(int playlistId, UpdatePlaylistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.UpdatePlaylistAsync] - Handling UpdatePlaylistAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new UpdatePlaylistCommand(playlistId, request.Name), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeletePlaylistAsync(int playlistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.DeletePlaylistAsync] - Handling DeletePlaylistAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new DeletePlaylistCommand(playlistId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> GetTracksByPlaylistIdAsync(int playlistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.GetTracksByPlaylistIdAsync] - Handling GetTracksByPlaylistIdAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new GetTracksByPlaylistIdQuery(playlistId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> AddTrackToPlaylistAsync(int playlistId, AddTrackToPlaylistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.AddTrackToPlaylistAsync] - Handling AddTrackToPlaylistAsync for PlaylistId: {PlaylistId}, TrackId: {TrackId}", playlistId, request.TrackId);
        var result = await mediator.Send(new AddTrackToPlaylistCommand(playlistId, request.TrackId), cancellationToken);
        return result.ToCreatedResult($"/api/v1/playlists/{playlistId}/tracks/{request.TrackId}");
    }

    private static async Task<IResult> RemoveTrackFromPlaylistAsync(int playlistId, int trackId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.RemoveTrackFromPlaylistAsync] - Handling RemoveTrackFromPlaylistAsync for PlaylistId: {PlaylistId}, TrackId: {TrackId}", playlistId, trackId);
        var result = await mediator.Send(new RemoveTrackFromPlaylistCommand(playlistId, trackId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { errors = result.Errors.Select(e => e.Message) });
    }

    private static async Task<IResult> ReorderPlaylistAsync(int playlistId, ReorderPlaylistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.ReorderPlaylistAsync] - Handling ReorderPlaylistAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new ReorderPlaylistCommand(playlistId, request.TrackIds), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ClonePlaylistAsync(int playlistId, ClonePlaylistRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.ClonePlaylistAsync] - Handling ClonePlaylistAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new ClonePlaylistCommand(playlistId, request.Name), cancellationToken);
        return result.ToCreatedResult($"/api/v1/playlists/{result.Value?.PlaylistId}");
    }

    private static async Task<IResult> GetPlaylistRecommendationsAsync(int playlistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[PlaylistEndpointExtensions.GetPlaylistRecommendationsAsync] - Handling GetPlaylistRecommendationsAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new GetPlaylistRecommendationsQuery(playlistId), cancellationToken);
        return result.ToHttpResult();
    }
}
