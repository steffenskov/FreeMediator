using FreeMediator.Handlers;
using FreeMediator.Requests;
using FreeMediator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FreeMediator.UnitTests;

public class SenderTests
{
	private readonly ISender _sender;

	public SenderTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config => { config.RegisterServicesFromAssemblyContaining<SenderTests>(); });

		var serviceProvider = services.BuildServiceProvider();
		_sender = serviceProvider.GetRequiredService<ISender>();
	}

	[Fact]
	public async Task Send_WithResponse_ReturnsResponse()
	{
		// Arrange
		var request = new EchoRequest("Hello world");

		// Act
		var result = await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(request.Message, result);
	}

	[Fact]
	public async Task Send_WithoutResponse_JustReturns()
	{
		// Arrange
		var request = new CommandRequest("Hello world");

		// Act
		await _sender.Send(request, TestContext.Current.CancellationToken);

		// Assert
		var handledMessage = Assert.Single(CommandHandler.HandledMessages);
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
	public static readonly List<string> HandledMessages = [];

	public Task Handle(CommandRequest request, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(request.Message);
		return Task.CompletedTask;
	}
}