using FluentResults;
using MediatR;

namespace Chinook.API.Common.Contracts.Queries;

/// <summary>
/// Generic query interface that returns a Result{T}, enabling consistent error handling.
/// </summary>
public interface IResultQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}
