namespace FreeMediator;

/// <summary>
///     Marker interface for requests with a response.
/// </summary>
public interface IRequest<out TResponse> : IBaseRequest
{
}

/// <summary>
///     Marker interface for requests without response.
/// </summary>
public interface IRequest : IBaseRequest
{
}

/// <summary>
///     Base marker interface, only used internally.
/// </summary>
public interface IBaseRequest
{
}