namespace FreeMediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken t = default);

public delegate Task NotificationHandlerDelegate(CancellationToken t = default);

/// <summary>
///     Implement a pipeline behavior which will be invoked prior to the request handler.
///     Pipeline behaviors are executed in the order they are registered.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse> : IBaseRequestPipelineBehavior<TResponse>
	where TRequest : IBaseRequest
{
	Task<TResponse> IBaseRequestPipelineBehavior<TResponse>.Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
///     Implement a pipeline behavior which will be invoked prior to the notification handler(s)
///     Will be invoked exactly once regardless of the number of handlers.
///     Pipeline behaviors are executed in the order they are registered.
/// </summary>
public interface IPipelineBehavior<in TNotification> : IBaseNotificationPipelineBehavior
	where TNotification : INotification
{
	Task IBaseNotificationPipelineBehavior.Handle(INotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		return Handle((TNotification)notification, next, cancellationToken);
	}

	/// <summary>
	///     Handler logic for the notification passing through the pipeline. Must call <paramref name="next" /> to pass the
	///     execution onto the next step of the pipeline.
	/// </summary>
	/// <param name="notification">Notification being sent through the mediator</param>
	/// <param name="next">Delegate for the next step of the pipeline, must be called!</param>
	/// <param name="cancellationToken">CancellationToken</param>
	Task Handle(TNotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken);
}