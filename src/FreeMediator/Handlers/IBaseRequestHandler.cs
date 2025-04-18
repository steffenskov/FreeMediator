namespace FreeMediator.Handlers;

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBaseRequestHandler
{
}

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBaseRequestHandler<TResponse> : IBaseRequestHandler
{
	Task<TResponse> Handle(IBaseRequest request, CancellationToken cancellationToken);
}