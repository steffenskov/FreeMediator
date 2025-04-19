namespace FreeMediator.UnitTests;

public class ExtensionTests
{
	[Fact]
	public void AddTransientDistinctImplementation_MultipleAddsOfSameType_OnlyAddedOnce()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();

		// Act
		serviceCollection.AddTransientDistinctImplementation(typeof(IService), typeof(ConcreteService));
		serviceCollection.AddTransientDistinctImplementation(typeof(IService), typeof(ConcreteService));

		var provider = serviceCollection.BuildServiceProvider();

		// Assert
		var services = provider.GetServices<IService>();
		Assert.Single(services);
	}

	[Fact]
	public void AddTransientDistinctImplementation_DifferentImplementations_AddsBoth()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();

		// Act
		serviceCollection.AddTransientDistinctImplementation(typeof(IService), typeof(ConcreteService));
		serviceCollection.AddTransientDistinctImplementation(typeof(IService), typeof(OtherService));

		var provider = serviceCollection.BuildServiceProvider();

		// Assert
		var services = provider.GetServices<IService>().ToList();
		Assert.Equal(2, services.Count);
		Assert.Contains(services, service => service.GetType() == typeof(ConcreteService));
		Assert.Contains(services, service => service.GetType() == typeof(OtherService));
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