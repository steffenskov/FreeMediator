using FreeMediator.ManualHandlerRegistration;

namespace FreeMediator.UnitTests.Configuration.MediatorConfigurationTests;

public partial class MediatorConfigurationTests
{
	[Fact]
	public void RegisterServicesFromAssemblyContaining_IgnoredRequestHandler_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.IgnoreServices(typeof(NestedGenericHandler<>));
		configuration.RegisterServicesFromAssemblyContaining<IMediatorHookup>();

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(NestedGenericHandler<>));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_IgnoredRequestHandlerAfterRegisterCall_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();
		configuration.RegisterServicesFromAssemblyContaining<IMediatorHookup>();
		configuration.IgnoreServices(typeof(NestedGenericHandler<>));

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(NestedGenericHandler<>));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_IgnoredNotificationHandler_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.IgnoreServices(typeof(NestedGenericHandler<>)); // Must also be ignored to avoid exceptions
		configuration.IgnoreServices(typeof(GenericNotificationHandler<>));
		configuration.RegisterServicesFromAssemblyContaining<IMediatorHookup>();

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(GenericNotificationHandler<>));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_IgnoredNotificationHandlerAfterRegisterCall_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.RegisterServicesFromAssemblyContaining<IMediatorHookup>();
		configuration.IgnoreServices(typeof(NestedGenericHandler<>)); // Must also be ignored to avoid exceptions
		configuration.IgnoreServices(typeof(GenericNotificationHandler<>));

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(GenericNotificationHandler<>));
	}

	[Fact]
	public void RegisterServicesFromAssemblyContaining_IgnoredViaPredicate_NotRegistered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.IgnoreServices(type => type.IsGenericType);
		configuration.RegisterServicesFromAssemblyContaining<IMediatorHookup>();

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(NestedGenericHandler<>));
		Assert.DoesNotContain(services, service => service.ImplementationType == typeof(GenericNotificationHandler<>));
	}

	[Fact]
	public void RegisterServices_IgnoredRequestHandler_Registered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.IgnoreServices(typeof(NestedGenericHandler<>));
		configuration.RegisterServices(typeof(NestedGenericHandler<string>));

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.Contains(services, service => service.ImplementationType == typeof(NestedGenericHandler<string>));
	}

	[Fact]
	public void RegisterServices_IgnoredNotificationHandler_Registered()
	{
		// Arrange
		var (configuration, services) = CreateConfiguration();

		configuration.IgnoreServices(typeof(GenericNotificationHandler<>));
		configuration.RegisterServices(typeof(GenericNotificationHandler<MyNotification>));

		// Act
		configuration.ExecuteAssemblyBasedRegistration();

		// Assert
		Assert.Contains(services, service => service.ImplementationType == typeof(GenericNotificationHandler<MyNotification>));
	}

	[Fact]
	public void IgnoreService_ClosedGenericType_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.IgnoreServices(typeof(GenericNotificationHandler<MyNotification>)));

		Assert.Equal("type must be a generic type definition (Parameter 'type')", ex.Message);
	}

	[Fact]
	public void IgnoreService_ArgumentNotAHandler_Throws()
	{
		// Arrange
		var (configuration, _) = CreateConfiguration();

		// Act && Assert
		var ex = Assert.Throws<ArgumentException>(() => configuration.IgnoreServices(typeof(string)));

		Assert.Equal("type must be an IRequestHandler or INotificationHandler (Parameter 'type')", ex.Message);
	}
}

file class MyNotification : INotification
{
}