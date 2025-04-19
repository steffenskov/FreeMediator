namespace FreeMediator.Configuration;

internal interface IServiceRegistrar : IEnumerable<ServiceDescriptor>
{
	void AddDistinctImplementation(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient);
	void AddDistinctService(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient);
}

internal class ServiceRegistrar : IServiceRegistrar
{
	private readonly IServiceCollection _services;

	public ServiceRegistrar(IServiceCollection services)
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
			throw new ArgumentException($"{implementationType.Name} is already registered", nameof(implementationType));
		}

		_services.Add(descriptor);
	}

	public void AddDistinctService(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
	{
		var descriptor = new ServiceDescriptor(service, implementationType, lifetime);

		var existingRegistration = _services.FirstOrDefault(registeredDescriptor => registeredDescriptor.ServiceType == descriptor.ServiceType
		                                                                            && Equals(registeredDescriptor.ServiceKey, descriptor.ServiceKey));

		if (existingRegistration is not null)
		{
			throw new ArgumentException($"{service.Name} already has a registered implementation ({existingRegistration.ImplementationType?.Name})", nameof(service));
		}

		_services.Add(descriptor);
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