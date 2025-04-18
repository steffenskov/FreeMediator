namespace FreeMediator.Configuration;

public class MediatorConfiguration
{
	private readonly IServiceCollection _services;

	internal MediatorConfiguration(IServiceCollection services)
	{
		_services = services;
	}

	#region RegisterServicesFromAssemblyContaining

	public MediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
	{
		return RegisterServicesFromAssemblyContaining(typeof(T));
	}

	public MediatorConfiguration RegisterServicesFromAssemblyContaining(Type markerType)
	{
		return RegisterServicesFromAssemblyContaining(markerType.Assembly);
	}

	public MediatorConfiguration RegisterServicesFromAssemblyContaining(Assembly assembly)
	{
		var types = assembly.GetTypes();
		foreach (var type in types)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				continue;
			}

			if (!type.IsAssignableTo(typeof(IBaseRequestHandler)) && !type.IsAssignableTo(typeof(IBaseNotificationHandler)))
			{
				continue;
			}

			if (type.IsGenericType)
			{
				throw new NotSupportedException("Generic handlers are not supported atm.");
			}

			var interfaces = type.GetInterfaces();

			var implementedResponseInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
			foreach (var implementedResponseInterface in implementedResponseInterfaces)
			{
				_services.TryAddTransient(implementedResponseInterface, type);
			}

			var implementedNoResponseInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<>));
			foreach (var implementedNoResponseInterface in implementedNoResponseInterfaces)
			{
				_services.TryAddTransient(implementedNoResponseInterface, type);
			}

			var implementedNotificationInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
			foreach (var implementedNotificationInterface in implementedNotificationInterfaces)
			{
				_services.AddTransientDistinctImplementation(implementedNotificationInterface, type);
			}
		}

		return this;
	}

	#endregion

	#region AddBehavior

	public MediatorConfiguration AddBehavior<TBehavior>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
		where TBehavior : IBasePipelineBehavior
	{
		return AddBehavior(typeof(TBehavior), serviceLifetime);
	}

	public MediatorConfiguration AddBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		var implementedInterfaces = implementationType.GetInterfaces()
			.Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
			.ToList();

		if (implementedInterfaces.Count == 0)
		{
			throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}");
		}

		foreach (var implementedInterface in implementedInterfaces)
		{
			_services.Add(ServiceDescriptor.Describe(implementedInterface, implementationType, serviceLifetime));
		}

		return this;
	}

	#endregion
}