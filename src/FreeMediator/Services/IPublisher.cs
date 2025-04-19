namespace FreeMediator;

/// <summary>
///     Used to publish a notification to one or more handlers.
/// </summary>
public interface IPublisher
{
	/// <summary>
	///     Publishes a notification to one or more handlers.
	///     Guarantees all handlers are invoked, but does not guarantee ordering of handlers.
	/// </summary>
	/// <param name="notification">Notification to publish</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	/// <exception cref="AggregateException">If any handler throws, the exception(s) will be rethrown as an AggregateException</exception>
	Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
		where TNotification : INotification;
}