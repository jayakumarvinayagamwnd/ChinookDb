using Chinook.API.Common.Results;
using MediatR;
using Serilog;

namespace Chinook.API.Features.Analytics;

public static class AnalyticsEndpointExtensions
{
    public static WebApplication MapAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/analytics").WithTags("Analytics");

        group.MapGet("/top-tracks", GetTopTracksAsync)
            .WithName("GetTopTracks")
            .WithSummary("Retrieves top-selling tracks.")
            .WithDescription("Returns top tracks ranked by units sold and revenue.")
            .Produces<List<TopTrackDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/top-artists", GetTopArtistsAsync)
            .WithName("GetTopArtists")
            .WithSummary("Retrieves top-selling artists.")
            .WithDescription("Returns top artists ranked by units sold and revenue.")
            .Produces<List<TopArtistDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/revenue-by-country", GetRevenueByCountryAsync)
            .WithName("GetRevenueByCountry")
            .WithSummary("Retrieves revenue by billing country.")
            .WithDescription("Returns aggregated revenue and invoice count by country.")
            .Produces<List<RevenueByCountryDto>>(StatusCodes.Status200OK);

        group.MapGet("/customer-ltv/{customerId:int}", GetCustomerLtvAsync)
            .WithName("GetCustomerLtv")
            .WithSummary("Retrieves lifetime value for a customer.")
            .WithDescription("Returns lifetime revenue metrics for a customer.")
            .Produces<CustomerLtvDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/sales-trend", GetSalesTrendAsync)
            .WithName("GetSalesTrend")
            .WithSummary("Retrieves sales trend by interval.")
            .WithDescription("Returns revenue trend grouped by day, month, or year.")
            .Produces<List<SalesTrendPointDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/cohort-retention", GetCohortRetentionAsync)
            .WithName("GetCohortRetention")
            .WithSummary("Retrieves customer retention by cohort.")
            .WithDescription("Returns monthly customer retention metrics by first-purchase cohort.")
            .Produces<List<CohortRetentionDto>>(StatusCodes.Status200OK);

        group.MapGet("/playlist-engagement/{playlistId:int}", GetPlaylistEngagementAsync)
            .WithName("GetPlaylistEngagement")
            .WithSummary("Retrieves playlist engagement metrics.")
            .WithDescription("Returns track count, units sold, revenue, and unique customers for a playlist.")
            .Produces<PlaylistEngagementDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetTopTracksAsync(int? limit, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetTopTracksAsync] - Handling GetTopTracksAsync with Limit: {Limit}", limit);
        var result = await mediator.Send(new GetTopTracksQuery(limit ?? 10), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTopArtistsAsync(int? limit, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetTopArtistsAsync] - Handling GetTopArtistsAsync with Limit: {Limit}", limit);
        var result = await mediator.Send(new GetTopArtistsQuery(limit ?? 10), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetRevenueByCountryAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetRevenueByCountryAsync] - Handling GetRevenueByCountryAsync");
        var result = await mediator.Send(new GetRevenueByCountryQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCustomerLtvAsync(int customerId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetCustomerLtvAsync] - Handling GetCustomerLtvAsync for CustomerId: {CustomerId}", customerId);
        var result = await mediator.Send(new GetCustomerLtvQuery(customerId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetSalesTrendAsync(string interval, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetSalesTrendAsync] - Handling GetSalesTrendAsync with Interval: {Interval}", interval);
        var result = await mediator.Send(new GetSalesTrendQuery(interval), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCohortRetentionAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetCohortRetentionAsync] - Handling GetCohortRetentionAsync");
        var result = await mediator.Send(new GetCohortRetentionQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPlaylistEngagementAsync(int playlistId, IMediator mediator, CancellationToken cancellationToken)
    {
        Log.Information("[AnalyticsEndpointExtensions.GetPlaylistEngagementAsync] - Handling GetPlaylistEngagementAsync for PlaylistId: {PlaylistId}", playlistId);
        var result = await mediator.Send(new GetPlaylistEngagementQuery(playlistId), cancellationToken);
        return result.ToHttpResult();
    }
}
