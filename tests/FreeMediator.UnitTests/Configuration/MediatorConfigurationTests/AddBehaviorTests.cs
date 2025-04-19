namespace FreeMediator.UnitTests.Configuration.MediatorConfigurationTests;

public partial class MediatorConfigurationTests
{
	[Fact]
	public void AddBehavior_GenericType_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddBehavior(typeof(GenericBehavior<,>)));

		Assert.Equal("implementationType cannot be a generic type (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddBehavior_DoesNotImplementIPipelineBehavior_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddBehavior(typeof(ClosedBehaviorWithoutInterfaceImplementation)));

		Assert.Equal($"{typeof(ClosedBehaviorWithoutInterfaceImplementation).Name} must implement {typeof(IPipelineBehavior<,>).Name} (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddBehavior_SameBehaviorTwice_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act
		configuration.AddBehavior<ClosedBehavior>();

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddBehavior<ClosedBehavior>());
		Assert.Equal($"{typeof(ClosedBehavior).Name} is already registered (Parameter 'implementationType')", ex.Message);
	}

	[Theory]
	[InlineData(ServiceLifetime.Transient)]
	[InlineData(ServiceLifetime.Scoped)]
	[InlineData(ServiceLifetime.Singleton)]
	public void AddBehavior_ProperBehavior_IsRegisteredWithCorrectLifetime(ServiceLifetime lifeTime)
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.AddBehavior<ClosedBehavior>(lifeTime);

		// Assert
		var descriptor = Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(ClosedBehavior));
		Assert.Equal(lifeTime, descriptor.Lifetime);
	}
}

file class FakeRequest : IRequest;

file class ClosedBehavior : IPipelineBehavior<FakeRequest, Unit>
{
	public Task<Unit> Handle(FakeRequest request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class GenericBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IBaseRequest
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class ClosedBehaviorWithoutInterfaceImplementation
{
}