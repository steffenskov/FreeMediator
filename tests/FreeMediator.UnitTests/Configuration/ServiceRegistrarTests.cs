namespace FreeMediator.UnitTests.Configuration;

public class ServiceRegistrarTests
{
	[Fact]
	public void AddTransientDistinctImplementation_MultipleAddsOfSameType_Throws()
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
	public void AddTransientDistinctImplementation_DifferentImplementations_AddsBoth()
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
	public void AddTransientDistinctImplementation_DifferentLifespans_Throws()
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