namespace FreeMediator.Internals;

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBasePipelineBehavior
{
}

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBasePipelineBehavior<TResponse> : IBasePipelineBehavior
{
	Task<TResponse> Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}