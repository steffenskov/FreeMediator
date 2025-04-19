// ReSharper disable once CheckNamespace

namespace FreeMediator;

/// <summary>
///     Implement a handler for notifications of type <typeparamref name="TNotification" />.
/// </summary>
public interface INotificationHandler<in TNotification> : IBaseNotificationHandler
	where TNotification : INotification
{
	/// <summary>
	///     Handler logic for the notification.
	///     If the logic throws an exception, it'll be collected into an <see cref="AggregateException" /> and re-thrown.
	///     <param name="notification">Notification being published through the mediator</param>
	///     <param name="cancellationToken">CancellationToken</param>
	/// </summary>
	Task Handle(TNotification notification, CancellationToken cancellationToken);
}