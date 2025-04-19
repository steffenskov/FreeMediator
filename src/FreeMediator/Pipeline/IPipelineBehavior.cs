namespace FreeMediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

/// <summary>
///     Implement a pipeline behavior which will be invoked prior to the request handler.
///     Pipeline behaviors are executed in the order they are registered.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse> : IBasePipelineBehavior<TResponse>
	where TRequest : IBaseRequest
{
	Task<TResponse> IBasePipelineBehavior<TResponse>.Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		return Handle((TRequest)request, next, cancellationToken);
	}

	/// <summary>
	///     Handler logic for the request passing through the pipeline. Must call <paramref name="next" /> to pass the
	///     execution onto the next step of the pipeline.
	/// </summary>
	/// <param name="request">Request being sent through the mediator</param>
	/// <param name="next">Delegate for the next step of the pipeline, must be called!</param>
	/// <param name="cancellationToken">CancellationToken</param>
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