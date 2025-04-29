namespace FreeMediator.UnitTests.Pipeline;

public class PipelineBehaviorTests
{
	private readonly IMediator _mediator;

	public PipelineBehaviorTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config =>
		{
			config.AddBehavior<FirstRequestBehavior>();
			config.AddBehavior<SecondRequestBehavior>();
			config.AddOpenBehavior(typeof(OpenBehavior<,>));
			config.AddBehavior<FirstNotificationBehavior>();
			config.AddBehavior<SecondNotificationBehavior>();
			config.AddOpenBehavior(typeof(OpenBehavior<>));
		});

		var config = new MediatorConfiguration(new ForgivingServiceRegistrar(services));
		config.RegisterServicesFromAssemblyContaining<PipelineBehaviorTests>();

		var serviceProvider = services.BuildServiceProvider();
		_mediator = serviceProvider.GetRequiredService<IMediator>();
	}

	[Fact]
	public async Task Publish_WithTwoBehaviors_BehaviorsInvokedInCorrectOrder()
	{
		// Arrange
		var notification = new NotificationWithBehavior("Hello world");

		// Act
		await _mediator.Publish(notification);

		// Assert
		Assert.Equal(42, notification.FirstBehaviorProperty);
		Assert.Equal(43, notification.SecondBehaviorProperty);
	}

	[Fact]
	public async Task Publish_WithOpenBehavior_BehaviorInvoked()
	{
		// Arrange
		var notification = new NotificationWithOpenBehavior("Hello world");

		// Act
		await _mediator.Publish(notification);

		// Assert
		Assert.Equal(42, notification.BehaviorProperty);
	}

	[Fact]
	public async Task Send_WithTwoBehaviors_BehaviorsInvokedInCorrectOrder()
	{
		// Arrange
		var request = new RequestWithBehavior("Hello world");

		// Act
		await _mediator.Send(request);

		// Assert
		Assert.Equal(42, request.FirstBehaviorProperty);
		Assert.Equal(43, request.SecondBehaviorProperty);
	}

	[Fact]
	public async Task Send_WithOpenBehavior_BehaviorInvoked()
	{
		// Arrange
		var request = new RequestWithOpenBehavior("Hello world");

		// Act
		await _mediator.Send(request);

		// Assert
		Assert.Equal(42, request.BehaviorProperty);
	}
}

#region Notifications

file record NotificationWithBehavior(string Message) : INotification
{
	public int FirstBehaviorProperty { get; set; }
	public int SecondBehaviorProperty { get; set; }
}

file class NotificationWithBehaviorHandler : INotificationHandler<NotificationWithBehavior>
{
	public Task Handle(NotificationWithBehavior notification, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}

file class FirstNotificationBehavior : IPipelineBehavior<NotificationWithBehavior>
{
	public Task Handle(NotificationWithBehavior notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		notification.FirstBehaviorProperty = 42;
		return next(cancellationToken);
	}
}

file class SecondNotificationBehavior : IPipelineBehavior<NotificationWithBehavior>
{
	public Task Handle(NotificationWithBehavior notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		notification.SecondBehaviorProperty = notification.FirstBehaviorProperty + 1;
		return next(cancellationToken);
	}
}

file record NotificationWithOpenBehavior(string Message) : INotification, INotificationWithProperty
{
	public int BehaviorProperty { get; set; }
}

file class NotificationWithOpenBehaviorHandler : INotificationHandler<NotificationWithOpenBehavior>
{
	public Task Handle(NotificationWithOpenBehavior request, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}

file interface INotificationWithProperty
{
	int BehaviorProperty { get; set; }
}

file class OpenBehavior<TNotification> : IPipelineBehavior<TNotification>
	where TNotification : INotification, INotificationWithProperty
{
	public Task Handle(TNotification request, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		request.BehaviorProperty = 42;
		return next(cancellationToken);
	}
}

#endregion

#region Requests

file record RequestWithBehavior(string Message) : IRequest
{
	public int FirstBehaviorProperty { get; set; }
	public int SecondBehaviorProperty { get; set; }
}

file class RequestWithBehaviorHandler : IRequestHandler<RequestWithBehavior>
{
	public Task<Unit> Handle(RequestWithBehavior request, CancellationToken cancellationToken)
	{
		return Unit.Task;
	}
}

file class FirstRequestBehavior : IPipelineBehavior<RequestWithBehavior, Unit>
{
	public Task<Unit> Handle(RequestWithBehavior request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		request.FirstBehaviorProperty = 42;
		return next(cancellationToken);
	}
}

file class SecondRequestBehavior : IPipelineBehavior<RequestWithBehavior, Unit>
{
	public Task<Unit> Handle(RequestWithBehavior request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		request.SecondBehaviorProperty = request.FirstBehaviorProperty + 1;
		return next(cancellationToken);
	}
}

file record RequestWithOpenBehavior(string Message) : IRequest, IRequestWithProperty
{
	public int BehaviorProperty { get; set; }
}

file class RequestWithOpenBehaviorHandler : IRequestHandler<RequestWithOpenBehavior>
{
	public Task<Unit> Handle(RequestWithOpenBehavior request, CancellationToken cancellationToken)
	{
		return Unit.Task;
	}
}

file interface IRequestWithProperty
{
	int BehaviorProperty { get; set; }
}

file class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest, IRequestWithProperty
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		request.BehaviorProperty = 42;
		return next(cancellationToken);
	}
}

#endregion