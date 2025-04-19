namespace FreeMediator.Configuration;

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

	public void RegisterGenericRequestHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 0: throw new UnreachableException($"Generic type must have at least one argument: {type.Name}");
			case 1: // TODO: Wrap in handler with 2 args
				throw new NotImplementedException($"Generic request handlers with a single generic type argument is not yet supported: {type.Name}");
			case 2:
				AddDistinctImplementation(typeof(IRequestHandler<,>), type);
				break;
			default: throw new NotSupportedException($"Generic request handlers with more than 2 generic type arguments are not supported: {type.Name}");
		}
	}

	public void RegisterGenericNotificationHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 0: throw new UnreachableException($"Generic type must have at least one argument: {type.Name}");
			case 1:
				AddDistinctImplementation(typeof(INotificationHandler<>), type);
				break;
			default: throw new NotSupportedException($"Generic notification handlers with more than 1 generic type arguments are not supported: {type.Name}");
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