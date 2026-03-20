using MediatR;

namespace Chinook.API.Common.Contracts.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull
{
}
