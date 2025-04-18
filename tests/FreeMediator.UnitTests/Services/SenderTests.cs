using Microsoft.Extensions.DependencyInjection;

namespace FreeMediator.UnitTests.Services;

public class SenderTests
{
	private readonly ISender _sender;

	public SenderTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config =>
		{
			config.RegisterServicesFromAssemblyContaining<SenderTests>();
			config.AddBehavior<FirstBehavior>();
			config.AddBehavior<SecondBehavior>();
			config.AddOpenBehavior(typeof(OpenBehavior<,>));
		});
		var serviceProvider = services.BuildServiceProvider();
		_sender = serviceProvider.GetRequiredService<ISender>();
	}

	[Fact]
	public async Task Send_WithResponse_ReturnsResponse()
	{
		// Arrange
		var request = new EchoRequest($"Hello world {Random.Shared.Next()}");

		// Act
		var result = await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(request.Message, result);
	}

	[Fact]
	public async Task Send_WithoutResponse_JustReturns()
	{
		// Arrange
		var request = new CommandRequest($"Hello world {Random.Shared.Next()}");

		// Act
		await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		var handledMessage = Assert.Single(CommandHandler.HandledMessages);
		Assert.Equal(request.Message, handledMessage);
	}

	[Fact]
	public async Task Send_UsesMultiHandler_IsInvoked()
	{
		// Arrange
		var firstCommand = new FirstMultiCommand($"Hello world {Random.Shared.Next()}");
		var secondCommand = new SecondMultiCommand($"Hello world {Random.Shared.Next()}");

		// Act
		await _sender.Send(firstCommand, TestContext.Current.CancellationToken);
		await _sender.Send(secondCommand, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(2, MultiCommandHandler.HandledMessages.Count);
		Assert.Contains(MultiCommandHandler.HandledMessages, message => message == firstCommand.Message);
		Assert.Contains(MultiCommandHandler.HandledMessages, message => message == secondCommand.Message);

		// Cleanup
		MultiCommandHandler.HandledMessages.Clear();
	}

	[Fact]
	public async Task Send_UsesMultiHandlerWithResponse_IsInvoked()
	{
		// Arrange
		var firstRequest = new FirstMultiRequestWithResponse($"Hello world {Random.Shared.Next()}");
		var secondRequest = new SecondMultiRequestWithResponse(Random.Shared.Next());

		// Act
		var firstResult = await _sender.Send(firstRequest, TestContext.Current.CancellationToken);
		var secondResult = await _sender.Send(secondRequest, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(firstRequest.Message, firstResult);
		Assert.Equal(secondRequest.Value, secondResult);
	}

	[Fact]
	public async Task Send_UsesMixedHandler_IsInvoked()
	{
		// Arrange
		var command = new MixedCommand($"Hello world {Random.Shared.Next()}");
		var request = new MixedRequest($"Hello world {Random.Shared.Next()}");

		// Act
		await _sender.Send(command, TestContext.Current.CancellationToken);
		var secondResult = await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		var handledCommand = Assert.Single(MixedRequestHandler.HandledMessages);
		Assert.Equal(command.Message, handledCommand);
		Assert.Equal(request.Message, secondResult);
	}

	[Fact]
	public async Task Send_WithTwoBehaviors_BehaviorsInvokedInCorrectOrder()
	{
		// Arrange
		var request = new RequestWithBehavior("Hello world");

		// Act
		await _sender.Send(request, TestContext.Current.CancellationToken);

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
		await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(42, request.BehaviorProperty);
	}
}

file record EchoRequest(string Message) : IRequest<string>;

file class EchoHandler : IRequestHandler<EchoRequest, string>
{
	public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
	{
		return Task.FromResult(request.Message);
	}
}

file record CommandRequest(string Message) : IRequest;

file class CommandHandler : IRequestHandler<CommandRequest>
{
	public static List<string> HandledMessages { get; } = [];

	public Task<Unit> Handle(CommandRequest request, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(request.Message);
		return Unit.Task;
	}
}

file record FirstMultiCommand(string Message) : IRequest;

file record SecondMultiCommand(string Message) : IRequest;

file class MultiCommandHandler : IRequestHandler<FirstMultiCommand>, IRequestHandler<SecondMultiCommand>
{
	public static List<string> HandledMessages { get; } = [];

	public Task<Unit> Handle(FirstMultiCommand command, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(command.Message);
		return Unit.Task;
	}

	public Task<Unit> Handle(SecondMultiCommand command, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(command.Message);
		return Unit.Task;
	}
}

file record FirstMultiRequestWithResponse(string Message) : IRequest<string>;

file record SecondMultiRequestWithResponse(int Value) : IRequest<int>;

file class MultiRequestWithResponseHandler : IRequestHandler<FirstMultiRequestWithResponse, string>, IRequestHandler<SecondMultiRequestWithResponse, int>
{
	public Task<string> Handle(FirstMultiRequestWithResponse request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(request.Message);
	}

	public Task<int> Handle(SecondMultiRequestWithResponse request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(request.Value);
	}
}

file record MixedCommand(string Message) : IRequest;

file record MixedRequest(string Message) : IRequest<string>;

file class MixedRequestHandler : IRequestHandler<MixedCommand>, IRequestHandler<MixedRequest, string>
{
	public static List<string> HandledMessages { get; } = [];

	public Task<Unit> Handle(MixedCommand request, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(request.Message);
		return Unit.Task;
	}

	public Task<string> Handle(MixedRequest request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(request.Message);
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