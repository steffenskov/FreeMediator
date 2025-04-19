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

		return await InvokePipelineAsync(request, t => service.Handle(request, t), cancellationToken);
	}

	public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
		where TRequest : IRequest
	{
		ArgumentNullException.ThrowIfNull(request);

		var service = _serviceProvider.GetService<IRequestHandler<TRequest>>();
		if (service is not null)
		{
			await InvokePipelineAsync(request, t => service.Handle(request, t), cancellationToken);
		}
		else // Maybe the user implemented the handler as IRequestHandler<TRequest, Unit>, let's check
		{
			await InvokePipelineAsync(request, t => _serviceProvider.GetRequiredService<IRequestHandler<TRequest, Unit>>().Handle(request, t), cancellationToken);
		}
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

	private async Task<TResponse> InvokePipelineAsync<TResponse>(IBaseRequest request, RequestHandlerDelegate<TResponse> serviceHandler, CancellationToken cancellationToken)
	{
		var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
		var behaviors = _serviceProvider.GetServices(pipelineBehaviorType).Cast<IBasePipelineBehavior<TResponse>>();

		var pipeline = behaviors
			.Reverse() // necessary to archive in-DI-order execution, due to the way they're piped together
			.Aggregate(serviceHandler, (next, pipeline) => t => pipeline.Handle(request, next, t));

		return await pipeline(cancellationToken);
	}
}