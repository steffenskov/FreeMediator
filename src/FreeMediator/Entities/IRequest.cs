namespace FreeMediator;

/// <summary>
///     Marker interface for requests with a response.
/// </summary>
public interface IRequest<out TResponse> : IBaseRequest
{
}

/// <summary>
///     Marker interface for requests without response, forces the return of Unit in the request handler as void cannot be
///     specified.
/// </summary>
public interface IRequest : IRequest<Unit>
{
}

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBaseRequest
{
}