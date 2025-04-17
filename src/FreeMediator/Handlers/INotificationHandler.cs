namespace FreeMediator.Handlers;

public interface INotificationHandler<in TNotification> : IBaseNotificationHandler
	where TNotification : INotification
{
	Task IBaseNotificationHandler.Handle(INotification notification, CancellationToken cancellationToken)
	{
		return Handle((TNotification)notification, cancellationToken);
	}

	Task Handle(TNotification notification, CancellationToken cancellationToken);
}

public interface IBaseNotificationHandler
{
	Task Handle(INotification notification, CancellationToken cancellationToken);
}