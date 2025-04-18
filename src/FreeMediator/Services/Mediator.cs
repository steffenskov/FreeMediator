namespace FreeMediator;

internal class Mediator : IMediator
{
	private readonly IServiceProvider _serviceProvider;

	public Mediator(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

		var service = (IBaseRequestHandler<TResponse>)_serviceProvider.GetRequiredService(handlerType);

		return await service.Handle(request, cancellationToken);
	}

	public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
		where TRequest : IRequest
	{
		ArgumentNullException.ThrowIfNull(request);

		var service = _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

		await service.Handle(request, cancellationToken);
	}

	public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
		where TNotification : INotification
	{
		ArgumentNullException.ThrowIfNull(notification);

		var services = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

		List<Exception> exceptions = [];

		foreach (var service in services)
		{
			try
			{
				await service.Handle(notification, cancellationToken);
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count > 0)
		{
			throw new AggregateException(exceptions);
		}
	}
}