namespace FreeMediator.UnitTests.Configuration;

public class ServiceRegistrarTests
{
	[Fact]
	public void AddDistinctImplementation_MultipleAddsOfSameType_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act
		registrar.AddDistinctImplementation(typeof(IService), typeof(ConcreteService));

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => registrar.AddDistinctImplementation(typeof(IService), typeof(ConcreteService)));
		Assert.Equal($"{typeof(ConcreteService).Name} is already registered (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddDistinctImplementation_DifferentImplementations_AddsBoth()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		var registrar = new ServiceRegistrar(serviceCollection);

		// Act
		registrar.AddDistinctImplementation(typeof(IService), typeof(ConcreteService));
		registrar.AddDistinctImplementation(typeof(IService), typeof(OtherService));

		var provider = serviceCollection.BuildServiceProvider();

		// Assert
		var services = provider.GetServices<IService>().ToList();
		Assert.Equal(2, services.Count);
		Assert.Contains(services, service => service.GetType() == typeof(ConcreteService));
		Assert.Contains(services, service => service.GetType() == typeof(OtherService));
	}

	[Fact]
	public void AddDistinctImplementation_DifferentLifespans_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act
		registrar.AddDistinctImplementation(typeof(IService), typeof(ConcreteService), ServiceLifetime.Scoped);

		// Assert
		// Assert
		var ex = Assert.Throws<ArgumentException>(() => registrar.AddDistinctImplementation(typeof(IService), typeof(ConcreteService), ServiceLifetime.Singleton));
		Assert.Equal($"{typeof(ConcreteService).Name} is already registered (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddDistinctService_NotRegistered_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		var registrar = new ServiceRegistrar(services);

		// Act
		registrar.AddDistinctService(typeof(IService), typeof(ConcreteService));

		// Assert
		Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IService));
	}

	[Fact]
	public void AddDistinctService_AlreadyRegistered_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		var registrar = new ServiceRegistrar(services);
		registrar.AddDistinctService(typeof(IService), typeof(ConcreteService));

		// Act
		var ex = Assert.Throws<ArgumentException>(() => registrar.AddDistinctService(typeof(IService), typeof(ConcreteService)));
		Assert.Equal($"{typeof(IService).Name} already has a registered implementation ({typeof(ConcreteService).Name}) (Parameter 'service')", ex.Message);
	}

	[Fact]
	public void GetEnumerator_WithoutGenerics_ReturnsSameEnumerator()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());
		registrar.RegisterGenericNotificationHandler(typeof(SingleGenericArgumentType<>));

		using var defaultEnumerator = registrar.GetEnumerator();

		// Act
		var nonGenericEnumerator = ((IEnumerable)registrar).GetEnumerator();

		// Assert
		Assert.Equal(defaultEnumerator, nonGenericEnumerator);
	}

	[Fact]
	public void RegisterGenericRequestHandler_NotGenericType_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act && Assert
		Assert.Throws<UnreachableException>(() => registrar.RegisterGenericRequestHandler(typeof(ConcreteService)));
	}

	[Fact]
	public void RegisterGenericRequestHandler_SingleGenericArgument_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act && Assert
		Assert.Throws<NotImplementedException>(() => registrar.RegisterGenericRequestHandler(typeof(SingleGenericArgumentType<>)));
	}

	[Fact]
	public void RegisterGenericRequestHandler_TwoGenericArgument_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		var registrar = new ServiceRegistrar(services);

		// Act
		registrar.RegisterGenericRequestHandler(typeof(TwoGenericArgumentType<,>));

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(TwoGenericArgumentType<,>));
	}

	[Fact]
	public void RegisterGenericRequestHandler_ThreeGenericArguments_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act && Assert
		var ex = Assert.Throws<NotSupportedException>(() => registrar.RegisterGenericRequestHandler(typeof(ThreeGenericArgumentType<,,>)));
		Assert.Equal($"Generic request handlers with more than 2 generic type arguments are not supported: {typeof(ThreeGenericArgumentType<,,>).Name}", ex.Message);
	}

	[Fact]
	public void RegisterGenericNotificationHandler_NotGenericType_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act && Assert
		Assert.Throws<UnreachableException>(() => registrar.RegisterGenericNotificationHandler(typeof(ConcreteService)));
	}

	[Fact]
	public void RegisterGenericNotificationHandler_SingleGenericArgument_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		var registrar = new ServiceRegistrar(services);

		// Act
		registrar.RegisterGenericNotificationHandler(typeof(SingleGenericArgumentType<>));

		// Assert
		Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(SingleGenericArgumentType<>));
	}

	[Fact]
	public void RegisterGenericNotificationHandler_TwoGenericArgument_Throws()
	{
		// Arrange
		var registrar = new ServiceRegistrar(new ServiceCollection());

		// Act && Assert
		var ex = Assert.Throws<NotSupportedException>(() => registrar.RegisterGenericNotificationHandler(typeof(TwoGenericArgumentType<,>)));
		Assert.Equal($"Generic notification handlers with more than 1 generic type arguments are not supported: {typeof(TwoGenericArgumentType<,>).Name}", ex.Message);
	}
}

file interface IService
{
}

file class ConcreteService : IService
{
}

file class OtherService : IService
{
}

file class SingleGenericArgumentType<TNotification> : INotificationHandler<TNotification>
	where TNotification : IRequestMarker, INotification
{
	public Task Handle(TNotification notification, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class TwoGenericArgumentType<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
	where TRequest : IRequestMarker, IRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class ThreeGenericArgumentType<T, T2, T3>
{
}

file interface IRequestMarker
{
}