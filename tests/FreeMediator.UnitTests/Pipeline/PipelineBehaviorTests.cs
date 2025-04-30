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

			config.AddBehavior<NotificationWithPostProcessingBehavior>();
			config.AddBehavior<RequestWithPostProcessingBehavior>();
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
	public async Task Publish_WithPostProcessingBehavior_BehaviorInvokedAfterHandler()
	{
		// Arrange
		var notification = new NotificationWithPostProcessing { Message = "Hello world" };

		// Act
		await _mediator.Publish(notification);

		// Assert
		Assert.Equal("Hello world", NotificationWithPostProcessingHandler.ReceivedMessage);
		Assert.Equal("Mutated by post-processing behavior", notification.Message);
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

	[Fact]
	public async Task Send_WithPostProcessingBehavior_BehaviorInvokedAfterHandler()
	{
		// Arrange
		var request = new RequestWithPostProcessing { Message = "Hello world" };

		// Act
		await _mediator.Send(request);

		// Assert
		Assert.Equal("Hello world", RequestWithPostProcessingHandler.ReceivedMessage);
		Assert.Equal("Mutated by post-processing behavior", request.Message);
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

file record NotificationWithPostProcessing : INotification
{
	public required string Message { get; set; }
}

file class NotificationWithPostProcessingHandler : INotificationHandler<NotificationWithPostProcessing>
{
	public static string? ReceivedMessage { get; private set; }

	public Task Handle(NotificationWithPostProcessing notification, CancellationToken cancellationToken)
	{
		ReceivedMessage = notification.Message;
		return Task.CompletedTask;
	}
}

file class NotificationWithPostProcessingBehavior : IPipelineBehavior<NotificationWithPostProcessing>
{
	public async Task Handle(NotificationWithPostProcessing notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		await next(cancellationToken);
		notification.Message = "Mutated by post-processing behavior";
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
	public Task Handle(RequestWithBehavior request, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
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
	public Task Handle(RequestWithOpenBehavior request, CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
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

file record RequestWithPostProcessing : IRequest
{
	public required string Message { get; set; }
}

file class RequestWithPostProcessingHandler : IRequestHandler<RequestWithPostProcessing>
{
	public static string? ReceivedMessage { get; private set; }

	public Task Handle(RequestWithPostProcessing notification, CancellationToken cancellationToken)
	{
		ReceivedMessage = notification.Message;
		return Task.CompletedTask;
	}
}

file class RequestWithPostProcessingBehavior : IPipelineBehavior<RequestWithPostProcessing, Unit>
{
	public async Task<Unit> Handle(RequestWithPostProcessing notification, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		await next(cancellationToken);
		notification.Message = "Mutated by post-processing behavior";

		return Unit.Value;
	}
}

#endregion