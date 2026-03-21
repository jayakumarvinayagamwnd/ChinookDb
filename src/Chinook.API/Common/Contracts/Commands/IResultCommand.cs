using FluentResults;
using MediatR;

namespace Chinook.API.Common.Contracts.Commands;

/// <summary>
/// Generic command interface that returns a Result{T}, enabling consistent error handling.
/// </summary>
public interface IResultCommand<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}
