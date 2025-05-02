namespace FreeMediator.UnitTests;

public class ExtensionTests
{
	[Fact]
	public void GetRequestHandlerInterface_DoesNotImplementInterface_Throws()
	{
		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => typeof(NonHandler).GetRequestHandlerInterface());
	}

	[Fact]
	public void GetRequestHandlerInterface_ImplementsInterface_ReturnsType()
	{
		// Act
		var result = typeof(CommandHandler).GetRequestHandlerInterface();

		// Assert
		Assert.Equal(typeof(IRequestHandler<Command>), result.InterfaceType);
		Assert.Equal(typeof(IRequestHandler<>), result.GenericTypeDefinition);
	}

	[Fact]
	public void GetRequestHandlerInterface_ImplementsGenericInterface_ReturnsType()
	{
		// Act
		var result = typeof(RequestHandler).GetRequestHandlerInterface();

		// Assert
		Assert.Equal(typeof(IRequestHandler<Request, string>), result.InterfaceType);
		Assert.Equal(typeof(IRequestHandler<,>), result.GenericTypeDefinition);
	}

	[Fact]
	public void GetRequestHandlerInterface_ImplementsMixedInterface_ReturnsType()
	{
		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => typeof(MixedHandler).GetRequestHandlerInterface());
	}
}

file class NonHandler
{
}

file class Command : IRequest;

file class CommandHandler : IRequestHandler<Command>
{
	public Task Handle(Command request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file record Request(string Message) : IRequest<string>;

file class RequestHandler : IRequestHandler<Request, string>
{
	public Task<string> Handle(Request request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class MixedHandler : IRequestHandler<Command>, IRequestHandler<Request, string>
{
	public Task Handle(Command request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<string> Handle(Request request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}