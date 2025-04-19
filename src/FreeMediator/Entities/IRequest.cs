namespace FreeMediator;

/// <summary>
///     Marker interface for requests with a response.
/// </summary>
public interface IRequest<out TResponse> : IBaseRequest
{
}

/// <summary>
///     Marker interface for requests without response, forces the return of <see cref="Unit" /> in the request handler
///     as void cannot be specified.
/// </summary>
public interface IRequest : IRequest<Unit>
{
}