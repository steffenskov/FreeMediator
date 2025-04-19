namespace FreeMediator.UnitTests.Pipeline;

public class PipelineBehaviorTests
{
	private readonly ISender _sender;

	public PipelineBehaviorTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config =>
		{
			config.AddBehavior<FirstBehavior>();
			config.AddBehavior<SecondBehavior>();
			config.AddOpenBehavior(typeof(OpenBehavior<,>));
		});

		var config = new MediatorConfiguration(new ForgivingServiceRegistrar(services));
		config.RegisterServicesFromAssemblyContaining<PipelineBehaviorTests>();

		var serviceProvider = services.BuildServiceProvider();
		_sender = serviceProvider.GetRequiredService<ISender>();
	}


	[Fact]
	public async Task Send_WithTwoBehaviors_BehaviorsInvokedInCorrectOrder()
	{
		// Arrange
		var request = new RequestWithBehavior("Hello world");

		// Act
		await _sender.Send(request);

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
		await _sender.Send(request);

		// Assert
		Assert.Equal(42, request.BehaviorProperty);
	}
}

file record RequestWithBehavior(string Message) : IRequest
{
	public int FirstBehaviorProperty { get; set; }
	public int SecondBehaviorProperty { get; set; }
}

file class RequestWithBehaviorHandler : IRequestHandler<RequestWithBehavior>
{
	public Task<Unit> Handle(RequestWithBehavior request, CancellationToken cancellationToken = default)
	{
		return Unit.Task;
	}
}

file class FirstBehavior : IPipelineBehavior<RequestWithBehavior, Unit>
{
	public Task<Unit> Handle(RequestWithBehavior request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		request.FirstBehaviorProperty = 42;
		return next(cancellationToken);
	}
}

file class SecondBehavior : IPipelineBehavior<RequestWithBehavior, Unit>
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
	public Task<Unit> Handle(RequestWithOpenBehavior request, CancellationToken cancellationToken = default)
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