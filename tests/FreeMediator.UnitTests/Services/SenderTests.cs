namespace FreeMediator.UnitTests.Services;

public class SenderTests
{
	private readonly ISender _sender;

	public SenderTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config => { });

		var config = new MediatorConfiguration(new ForgivingServiceRegistrar(services));
		config.RegisterServicesFromAssemblyContaining<SenderTests>();

		var serviceProvider = services.BuildServiceProvider();
		_sender = serviceProvider.GetRequiredService<ISender>();
	}

	[Fact]
	public async Task Send_WithResponse_ReturnsResponse()
	{
		// Arrange
		var request = new EchoRequest($"Hello world {Random.Shared.Next()}");

		// Act
		var result = await _sender.Send(request);

		// Assert
		Assert.Equal(request.Message, result);
	}

	[Fact]
	public async Task Send_WithoutResponse_JustReturns()
	{
		// Arrange
		var request = new CommandRequest($"Hello world {Random.Shared.Next()}");

		// Act
		await _sender.Send(request);

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
		await _sender.Send(firstCommand);
		await _sender.Send(secondCommand);

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
		var firstResult = await _sender.Send(firstRequest);
		var secondResult = await _sender.Send(secondRequest);

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
		await _sender.Send(command);
		var secondResult = await _sender.Send(request);

		// Assert
		var handledCommand = Assert.Single(MixedRequestHandler.HandledMessages);
		Assert.Equal(command.Message, handledCommand);
		Assert.Equal(request.Message, secondResult);
	}

	[Fact]
	public async Task Send_GenericRequest_IsHandled()
	{
		// Arrange
		var intRequest = new GenericEchoRequest<int>(42);
		var stringRequest = new GenericEchoRequest<string>($"Hello world {Random.Shared.Next()}");

		// Act
		var intResult = await _sender.Send(intRequest);
		var stringResult = await _sender.Send(stringRequest);

		// Assert
		Assert.Equal(intRequest.Value, intResult);
		Assert.Equal(stringRequest.Value, stringResult);
	}

	[Fact]
	public async Task Send_RequestWithWrongHandlerType_StillHandled()
	{
		// Arrange
		var request = new WrongHandlerCommand($"Hello world {Random.Shared.Next()}");

		// Act
		await _sender.Send(request);

		// Assert
		var handledMessage = Assert.Single(WrongHandler.HandledMessages);
		Assert.Equal(request.Message, handledMessage);
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

	public Task Handle(CommandRequest request, CancellationToken cancellationToken)
	{
		HandledMessages.Add(request.Message);
		return Task.CompletedTask;
	}
}

file record FirstMultiCommand(string Message) : IRequest;

file record SecondMultiCommand(string Message) : IRequest;

file class MultiCommandHandler : IRequestHandler<FirstMultiCommand>, IRequestHandler<SecondMultiCommand>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(FirstMultiCommand command, CancellationToken cancellationToken)
	{
		HandledMessages.Add(command.Message);
		return Task.CompletedTask;
	}

	public Task Handle(SecondMultiCommand command, CancellationToken cancellationToken)
	{
		HandledMessages.Add(command.Message);
		return Task.CompletedTask;
	}
}

file record FirstMultiRequestWithResponse(string Message) : IRequest<string>;

file record SecondMultiRequestWithResponse(int Value) : IRequest<int>;

file class MultiRequestWithResponseHandler : IRequestHandler<FirstMultiRequestWithResponse, string>, IRequestHandler<SecondMultiRequestWithResponse, int>
{
	public Task<string> Handle(FirstMultiRequestWithResponse request, CancellationToken cancellationToken)
	{
		return Task.FromResult(request.Message);
	}

	public Task<int> Handle(SecondMultiRequestWithResponse request, CancellationToken cancellationToken)
	{
		return Task.FromResult(request.Value);
	}
}

file record MixedCommand(string Message) : IRequest;

file record MixedRequest(string Message) : IRequest<string>;

file class MixedRequestHandler : IRequestHandler<MixedCommand>, IRequestHandler<MixedRequest, string>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(MixedCommand request, CancellationToken cancellationToken)
	{
		HandledMessages.Add(request.Message);
		return Task.CompletedTask;
	}

	public Task<string> Handle(MixedRequest request, CancellationToken cancellationToken)
	{
		return Task.FromResult(request.Message);
	}
}

file record GenericEchoRequest<T>(T Value) : IRequest<T>;

file class GenericEchoRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
	where TRequest : GenericEchoRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
	{
		return Task.FromResult(request.Value);
	}
}

file record WrongHandlerCommand(string Message) : IRequest;

file class WrongHandler : IRequestHandler<WrongHandlerCommand, Unit> // Should had just used IRequestHandler<TRequest>
{
	public static List<string> HandledMessages { get; } = [];

	public Task<Unit> Handle(WrongHandlerCommand request, CancellationToken cancellationToken)
	{
		HandledMessages.Add(request.Message);
		return Unit.Task;
	}
}