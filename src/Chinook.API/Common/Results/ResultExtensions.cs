using System.Linq;
using FluentResults;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Chinook.API.Common.Results;

/// <summary>
/// Extensions for mapping FluentResults Result{T} to ASP.NET Core IResult HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a FluentResults Result{T} to an IResult, mapping success to 200 OK and failures to 400 Bad Request.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result) where T : notnull
    {
        if (result.IsSuccess)
            return HttpResults.Ok(result.Value);

        var errors = result.Errors
            .Select(e => new { error = e.Message })
            .ToList();

        return HttpResults.BadRequest(new { errors });
    }

    /// <summary>
    /// Converts a FluentResults Result to an IResult (no payload), mapping success to 204 No Content.
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return HttpResults.NoContent();

        var errors = result.Errors
            .Select(e => new { error = e.Message })
            .ToList();

        return HttpResults.BadRequest(new { errors });
    }

    /// <summary>
    /// Converts a FluentResults Result{T} to an IResult with custom status codes.
    /// Maps success to the provided successStatus; failures to 400 Bad Request.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, int successStatus) where T : notnull
    {
        if (result.IsSuccess)
            return HttpResults.Json(result.Value, statusCode: successStatus);

        var errors = result.Errors
            .Select(e => new { error = e.Message })
            .ToList();

        return HttpResults.BadRequest(new { errors });
    }

    /// <summary>
    /// Converts a FluentResults Result{T?} to an IResult, mapping null to 404 Not Found.
    /// </summary>
    public static IResult ToHttpResultOrNotFound<T>(this Result<T?> result) where T : notnull
    {
        if (!result.IsSuccess)
        {
            var errors = result.Errors
                .Select(e => new { error = e.Message })
                .ToList();
            return HttpResults.BadRequest(new { errors });
        }

        if (result.Value is null)
            return HttpResults.NotFound();

        return HttpResults.Ok(result.Value);
    }

    /// <summary>
    /// Converts a FluentResults Result{T} to an IResult with 201 Created status on success.
    /// </summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, string locationTemplate) where T : notnull
    {
        if (!result.IsSuccess)
        {
            var errors = result.Errors
                .Select(e => new { error = e.Message })
                .ToList();
            return HttpResults.BadRequest(new { errors });
        }

        return HttpResults.Created(locationTemplate, result.Value);
    }

    /// <summary>
    /// Adds an error to a result with a descriptive message.
    /// </summary>
    public static Result<T> Fail<T>(this Result<T> result, string message) where T : notnull
    {
        return result.WithError(message);
    }

    /// <summary>
    /// Adds an error to a result.
    /// </summary>
    public static Result Fail(this Result result, string message)
    {
        return result.WithError(message);
    }
}
