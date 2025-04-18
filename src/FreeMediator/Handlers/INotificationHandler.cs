// ReSharper disable once CheckNamespace

namespace FreeMediator;

public interface INotificationHandler<in TNotification> : IBaseNotificationHandler
	where TNotification : INotification
{
	Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}