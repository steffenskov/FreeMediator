namespace FreeMediator.UnitTests.Configuration.MediatorConfigurationTests;

public partial class MediatorConfigurationTests
{
	[Fact]
	public void RegisterServicesFromAssemblyContaining_GenericArgument_Registers()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration(true);

		// Act
		configuration.RegisterServicesFromAssemblyContaining<FakeCommand>();

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeCommandHandler));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_TypeArgument_Registers()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration(true);

		// Act
		configuration.RegisterServicesFromAssemblyContaining(typeof(FakeCommand));

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeCommandHandler));
	}

	[Fact]
	public void RegisterServicesFromAssemblies_ProperAssembly_Registers()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration(true);
		var assembly = typeof(FakeCommand).Assembly;

		// Act
		configuration.RegisterServicesFromAssemblies(assembly);

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeCommandHandler));
	}

	[Fact]
	public void RegisterServicesFromAssembly_ProperAssembly_Registers()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration(true);
		var assembly = typeof(FakeCommand).Assembly;

		// Act
		configuration.RegisterServicesFromAssembly(assembly);

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeCommandHandler));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_ContainsBothRequestAndNotificationHandlers_RegistersThemAll()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration(true);

		// Act
		configuration.RegisterServicesFromAssemblyContaining<FakeCommand>();

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeCommandHandler));
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeRequestHandler));
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(FakeNotificationHandler));
	}

	[Fact]
	public void RegisterServices_SameNotificationHandlerTwice_Throws()
	{
		// Arrange
		var handlerType = typeof(FakeNotificationHandler);
		var (configuration, _) = CreateConfiguration();

		// Act
		configuration.RegisterServices(handlerType);

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.RegisterServices(handlerType));
		Assert.Equal($"{handlerType.Name} is already registered (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void RegisterServices_CommandHandlerWithSameServiceTypeTwice_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act
		configuration.RegisterServices(typeof(FakeCommandHandler));

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.RegisterServices(typeof(FakeCommandHandler2)));
		Assert.Equal($"{typeof(IRequestHandler<FakeCommand>).Name} already has a registered implementation ({typeof(FakeCommandHandler).Name}) (Parameter 'service')", ex.Message);
	}

	[Fact]
	public void RegisterServices_RequestHandlerWithSameServiceTypeTwice_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act
		configuration.RegisterServices(typeof(FakeRequestHandler));

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.RegisterServices(typeof(FakeRequestHandler2)));
		Assert.Equal($"{typeof(IRequestHandler<FakeRequest, string>).Name} already has a registered implementation ({typeof(FakeRequestHandler).Name}) (Parameter 'service')", ex.Message);
	}

	[Fact]
	public void RegisterServices_RequestHandlerWithJustOneArgument_Registers()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.RegisterServices(typeof(InvalidSingleArgumentGenericRequestHandler<>));

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(InvalidSingleArgumentGenericRequestHandler<>));
	}

	[Fact]
	public void RegisterServices_RequestHandlerWithMoreThanTwoArguments_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<NotSupportedException>(() => configuration.RegisterServices(typeof(InvalidTripleArgumentGenericRequestHandler<,,>)));
		Assert.Equal($"Generic request handlers with more than 2 generic type arguments are not supported: {typeof(InvalidTripleArgumentGenericRequestHandler<,,>).Name}", ex.Message);
	}

	[Fact]
	public void RegisterServices_GenericNotificationHandlerWithMoreThanOneArgument_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<NotSupportedException>(() => configuration.RegisterServices(typeof(InvalidGenericNotificationHandler<,>)));
		Assert.Equal($"Generic notification handlers with more than 1 generic type arguments are not supported: {typeof(InvalidGenericNotificationHandler<,>).Name}", ex.Message);
	}

	[Fact]
	public void RegisterServices_AbstractClass_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.RegisterServices(typeof(AbstractHandler<>));

		// Assert
		Assert.Empty(services);
	}

	[Fact]
	public void RegisterServices_Interface_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.RegisterServices(typeof(InterfaceHandler<>));

		// Assert
		Assert.Empty(services);
	}
}

file class FakeCommand : IRequest;

file class FakeCommandHandler : IRequestHandler<FakeCommand>
{
	public Task Handle(FakeCommand command, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class FakeCommandHandler2 : IRequestHandler<FakeCommand>
{
	public Task Handle(FakeCommand request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file record FakeRequest(string Message) : IRequest<string>;

file class FakeRequestHandler : IRequestHandler<FakeRequest, string>
{
	public Task<string> Handle(FakeRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class FakeRequestHandler2 : IRequestHandler<FakeRequest, string>
{
	public Task<string> Handle(FakeRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file record FakeNotification(string Message) : INotification;

file class FakeNotificationHandler : INotificationHandler<FakeNotification>
{
	public Task Handle(FakeNotification notification, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class InvalidGenericNotificationHandler<TNotification, T> : INotificationHandler<TNotification>
	where TNotification : INotification
{
	public Task Handle(TNotification notification, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class InvalidSingleArgumentGenericRequestHandler<TRequest> : IRequestHandler<TRequest>
	where TRequest : IRequest
{
	public Task Handle(TRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class InvalidTripleArgumentGenericRequestHandler<TRequest, TResponse, T> : IRequestHandler<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file abstract class AbstractHandler<TRequest> : IRequestHandler<TRequest> 
	where TRequest : IRequest
{
	public abstract Task Handle(TRequest request, CancellationToken cancellationToken);
}

file interface InterfaceHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
{
	
}