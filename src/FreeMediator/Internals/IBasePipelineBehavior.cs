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
public interface IBaseRequestPipelineBehavior<TResponse> : IBasePipelineBehavior
{
	Task<TResponse> Handle(IBaseRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

public interface IBaseNotificationPipelineBehavior : IBasePipelineBehavior
{
	Task Handle(INotification notification, NotificationHandlerDelegate next, CancellationToken cancellationToken);
}