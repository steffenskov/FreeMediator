using FreeMediator.Internals;

namespace FreeMediator.UnitTests.Configuration.MediatorConfigurationTests;

public partial class MediatorConfigurationTests
{
	[Fact]
	public void AddOpenBehavior_NotGenericType_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(ClosedBehavior)));

		Assert.Equal("implementationType must be a generic type (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddOpenBehavior_GenericTypeButNotDefinition_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(GenericBehavior<FakeRequest, Unit>)));

		Assert.Equal("implementationType must be a generic type definition (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddOpenBehavior_GenericTypeButNotTwoArguments_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(GenericBehaviorWithSingleArg<>)));

		Assert.Equal("implementationType must take the same number of type arguments (have the same arity) as the IPipelineBehavior<> or IPipelineBehavior<,> interface implemented. (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddOpenBehavior_ImplementsBothNotificationAndRequestIPipelineBehavior_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(GenericBehaviorWithBothInterfaceImplementations<,>)));

		Assert.Equal($"{typeof(GenericBehaviorWithBothInterfaceImplementations<FakeNotificationAndRequest, Unit>).Name} can only implement either IPipelineBehavior<> or IPipelineBehavior<,> (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddOpenBehavior_DoesNotImplementIPipelineBehavior_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(GenericBehaviorWithoutInterfaceImplementation<,>)));

		Assert.Equal($"{typeof(GenericBehaviorWithoutInterfaceImplementation<FakeRequest, Unit>).Name} must implement IPipelineBehavior<> or IPipelineBehavior<,> (Parameter 'implementationType')", ex.Message);
	}

	[Fact]
	public void AddOpenBehavior_SameBehaviorTwice_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act
		configuration.AddOpenBehavior(typeof(GenericBehavior<,>));

		// Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.AddOpenBehavior(typeof(GenericBehavior<,>)));
		Assert.Equal($"{typeof(GenericBehavior<,>).Name} is already registered (Parameter 'implementationType')", ex.Message);
	}

	[Theory]
	[InlineData(ServiceLifetime.Transient)]
	[InlineData(ServiceLifetime.Scoped)]
	[InlineData(ServiceLifetime.Singleton)]
	public void AddOpenBehavior_ProperBehavior_IsRegisteredWithCorrectLifetime(ServiceLifetime lifeTime)
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.AddOpenBehavior(typeof(GenericBehavior<,>), lifeTime);

		// Assert
		var descriptor = Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(GenericBehavior<,>));
		Assert.Equal(lifeTime, descriptor.Lifetime);
	}

	[Theory]
	[InlineData(ServiceLifetime.Transient)]
	[InlineData(ServiceLifetime.Scoped)]
	[InlineData(ServiceLifetime.Singleton)]
	public void AddOpenBehaviors_ProperBehaviors_AreRegisteredWithCorrectLifetime(ServiceLifetime lifeTime)
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		// Act
		configuration.AddOpenBehaviors([typeof(GenericBehavior<,>), typeof(OtherGenericBehavior<,>)], lifeTime);

		// Assert
		var genericBehaviorDescriptor = Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(GenericBehavior<,>));
		var otherGenericBehaviorDescriptor = Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(OtherGenericBehavior<,>));

		Assert.Equal(lifeTime, genericBehaviorDescriptor.Lifetime);
		Assert.Equal(lifeTime, otherGenericBehaviorDescriptor.Lifetime);
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
	where TRequest : IRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class OtherGenericBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class GenericBehaviorWithSingleArg<TRequest> : IPipelineBehavior<TRequest, Unit>
	where TRequest : IBaseRequest
{
	public Task<Unit> Handle(TRequest request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class GenericBehaviorWithoutInterfaceImplementation<TRequest, TResponse>
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}

file class FakeNotificationAndRequest : INotification, IRequest;

file class GenericBehaviorWithBothInterfaceImplementations<TRequest, TResponse> : IPipelineBehavior<TRequest>, IPipelineBehavior<TRequest, TResponse>
	where TRequest : IBaseRequest, INotification
{
	public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task Handle(TRequest notification, NotificationHandlerDelegate next, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}