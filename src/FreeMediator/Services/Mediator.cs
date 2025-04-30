namespace FreeMediator;

internal class Mediator : IMediator
{
	private readonly IServiceProvider _serviceProvider;

	public Mediator(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);

		var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
		var service = _serviceProvider.GetServices(handlerType).Cast<IBaseRequestHandler<TResponse>>().Single();

		return await InvokeRequestPipelineAsync(request, ct => service.Handle(request, ct), cancellationToken);
	}

	public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken)
		where TRequest : IRequest
	{
		ArgumentNullException.ThrowIfNull(request);

		var service = _serviceProvider.GetServices<IRequestHandler<TRequest>>().SingleOrDefault();

		if (service is not null)
		{
			RequestHandlerDelegate<Unit> serviceHandler = async ct =>
			{
				await service.Handle(request, ct);
				return Unit.Value;
			};

			await InvokeRequestPipelineAsync(request, serviceHandler, cancellationToken);
		}
		else // Maybe the user implemented the handler as IRequestHandler<TRequest, Unit>, let's check
		{
			await InvokeRequestPipelineAsync(request, ct => _serviceProvider.GetRequiredService<IRequestHandler<TRequest, Unit>>().Handle(request, ct), cancellationToken);
		}
	}


	public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
		where TNotification : INotification
	{
		ArgumentNullException.ThrowIfNull(notification);

		var services = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

		NotificationHandlerDelegate serviceHandler = async ct =>
		{
			List<Exception> exceptions = [];

			foreach (var service in services)
			{
				try
				{
					await service.Handle(notification, ct);
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
		};

		await InvokeNotificationPipelineAsync(notification, serviceHandler, cancellationToken);
	}

	private async Task<TResponse> InvokeRequestPipelineAsync<TResponse>(IBaseRequest request, RequestHandlerDelegate<TResponse> serviceHandler, CancellationToken cancellationToken)
	{
		var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
		var behaviors = _serviceProvider.GetServices(pipelineBehaviorType).Cast<IBaseRequestPipelineBehavior<TResponse>>();

		var pipeline = behaviors
			.Reverse() // necessary to archive in-DI-order execution, due to the way they're piped together
			.Aggregate(serviceHandler, (next, pipeline) => t => pipeline.Handle(request, next, t));

		return await pipeline(cancellationToken);
	}

	private async Task InvokeNotificationPipelineAsync(INotification notification, NotificationHandlerDelegate serviceHandler, CancellationToken cancellationToken)
	{
		var pipelineBehaviorType = typeof(IPipelineBehavior<>).MakeGenericType(notification.GetType());
		var behaviors = _serviceProvider.GetServices(pipelineBehaviorType).Cast<IBaseNotificationPipelineBehavior>();

		var pipeline = behaviors
			.Reverse() // necessary to archive in-DI-order execution, due to the way they're piped together
			.Aggregate(serviceHandler, (next, pipeline) => t => pipeline.Handle(notification, next, t));

		await pipeline(cancellationToken);
	}
}