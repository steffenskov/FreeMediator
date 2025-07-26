namespace FreeMediator.ManualHandlerRegistration;

public class GenericNotificationHandler<TRequest> : INotificationHandler<TRequest>
	where TRequest : INotification
{
	public Task Handle(TRequest notification, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}