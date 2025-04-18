namespace FreeMediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

public interface IPipelineBehavior<in TRequest, TResponse> : IBasePipelineBehavior<TResponse>
	where TRequest : IBaseRequest
{
	Task<TResponse> IBasePipelineBehavior<TResponse>.Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		return Handle((TRequest)request, next, cancellationToken);
	}

	Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBasePipelineBehavior<TResponse> : IBasePipelineBehavior
{
	Task<TResponse> Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
///     Base marker interface, only supposed to be used internally.
/// </summary>
public interface IBasePipelineBehavior
{
}