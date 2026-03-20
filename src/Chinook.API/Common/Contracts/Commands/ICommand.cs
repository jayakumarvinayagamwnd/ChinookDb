using MediatR;

namespace Chinook.API.Common.Contracts.Commands;

public interface ICommand<out TResponse> : IRequest<TResponse>
    where TResponse : notnull
{
}
