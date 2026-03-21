using FluentResults;
using MediatR;

namespace Chinook.API.Common.Contracts.Commands;

/// <summary>
/// Handler interface for result-based commands that return Result{T}.
/// </summary>
public interface IResultCommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : IResultCommand<TResponse>
    where TResponse : notnull
{
}
