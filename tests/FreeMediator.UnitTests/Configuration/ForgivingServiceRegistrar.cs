namespace FreeMediator.UnitTests.Configuration;

/// <summary>
///     Forgiving ServiceRegistrar that doesn't throw exceptions when breaking distinctiveness.
///     Purely used for testing purposes, to allow invalid assembly configurations with duplicates.
/// </summary>
internal class ForgivingServiceRegistrar : IServiceRegistrar
{
	private readonly IServiceCollection _services;

	public ForgivingServiceRegistrar(IServiceCollection services)
	{
		_services = services;
	}

	public void AddDistinctImplementation(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
	{
		var descriptor = new ServiceDescriptor(service, implementationType, lifetime);

		if (_services.Any(registeredDescriptor => registeredDescriptor.ServiceType == descriptor.ServiceType
		                                          && registeredDescriptor.ImplementationType == descriptor.ImplementationType
		                                          && Equals(registeredDescriptor.ServiceKey, descriptor.ServiceKey)))
		{
			return;
		}

		_services.Add(descriptor);
	}

	public void AddDistinctService(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
	{
		var descriptor = new ServiceDescriptor(service, implementationType, lifetime);

		if (_services.Any(registeredDescriptor => registeredDescriptor.ServiceType == descriptor.ServiceType
		                                          && Equals(registeredDescriptor.ServiceKey, descriptor.ServiceKey)))
		{
			return;
		}

		_services.Add(descriptor);
	}

	public void RegisterGenericRequestHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 2:
				AddDistinctImplementation(typeof(IRequestHandler<,>), type);
				break;
		}
	}

	public void RegisterGenericNotificationHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 1:
				AddDistinctImplementation(typeof(INotificationHandler<>), type);
				break;
		}
	}

	public IEnumerator<ServiceDescriptor> GetEnumerator()
	{
		return _services.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}