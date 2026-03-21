using FluentResults;
using MediatR;

namespace Chinook.API.Common.Contracts.Queries;

/// <summary>
/// Handler interface for result-based queries that return Result{T}.
/// </summary>
public interface IResultQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IResultQuery<TResponse>
    where TResponse : notnull
{
}
