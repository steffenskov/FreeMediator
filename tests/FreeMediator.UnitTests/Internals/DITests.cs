namespace FreeMediator.UnitTests.Internals;

/// <summary>
///     Tests the built-in DI in .Net to ensure any changes to it's inner workings don't break this library.
/// </summary>
public class DITests
{
	[Fact]
	public void GetRequiredService_MultipleOpenTypesRegistered_ThrowsDueToGenericConstraints()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddTransient(typeof(IOpenGenericInterface<>), typeof(EntityImplementation<>));
		services.AddTransient(typeof(IOpenGenericInterface<>), typeof(AggregateImplementation<>));
		var serviceProvider = services.BuildServiceProvider();

		// Act
		var entityException = Record.Exception(() => serviceProvider.GetRequiredService<IOpenGenericInterface<Entity>>());
		var aggregateException = Record.Exception(() => serviceProvider.GetRequiredService<IOpenGenericInterface<Aggregate>>());

		// Assert
		Assert.True(entityException is not null || aggregateException is not null);
	}

	[Fact]
	public void GetServices_MultipleOpenTypesRegistered_OnlyRetrievesThoseCompliantWithGenericConstraints()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddTransient(typeof(IOpenGenericInterface<>), typeof(EntityImplementation<>));
		services.AddTransient(typeof(IOpenGenericInterface<>), typeof(AggregateImplementation<>));
		var serviceProvider = services.BuildServiceProvider();

		// Act
		var entityServices = serviceProvider.GetServices(typeof(IOpenGenericInterface<Entity>)).ToList();
		var aggregateServices = serviceProvider.GetServices(typeof(IOpenGenericInterface<Aggregate>)).ToList();

		// Assert
		Assert.Single(entityServices);
		Assert.Single(aggregateServices);
		Assert.NotEqual(entityServices.First(), aggregateServices.First());
	}
}

file interface IOpenGenericInterface<T>
{
}

file interface IEntity
{
}

file interface IAggregate
{
}

file class Entity : IEntity
{
}

file class Aggregate : IAggregate
{
}

file class EntityImplementation<T> : IOpenGenericInterface<T>
	where T : IEntity
{
}

file class AggregateImplementation<T> : IOpenGenericInterface<T>
	where T : IAggregate
{
}