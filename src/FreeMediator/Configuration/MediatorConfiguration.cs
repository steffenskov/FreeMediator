namespace FreeMediator.Configuration;

internal class MediatorConfiguration : IMediatorConfiguration
{
	private readonly IServiceRegistrar _services;

	internal MediatorConfiguration(IServiceRegistrar services)
	{
		_services = services;
	}

	#region AddOpenBehavior

	public IMediatorConfiguration AddOpenBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		if (!implementationType.IsGenericType)
		{
			throw new ArgumentException($"{nameof(implementationType)} must be a generic type", nameof(implementationType));
		}

		if (!implementationType.IsGenericTypeDefinition)
		{
			throw new ArgumentException($"{nameof(implementationType)} must be a generic type definition", nameof(implementationType));
		}

		if (implementationType.GetGenericArguments().Length != 2)
		{
			throw new ArgumentException($"{nameof(implementationType)} must take 2 type arguments matching IPipelineBehavior<,>", nameof(implementationType));
		}

		var implementsPipeline = implementationType.GetInterfaces()
			.Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

		if (!implementsPipeline)
		{
			throw new ArgumentException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).Name}", nameof(implementationType));
		}

		if (_services.All(x => x.ServiceType != implementationType))

		{
			_services.AddDistinctImplementation(typeof(IPipelineBehavior<,>), implementationType, serviceLifetime);
		}

		return this;
	}

	public IMediatorConfiguration AddOpenBehaviors(IEnumerable<Type> openBehaviorTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		foreach (var openBehaviorType in openBehaviorTypes)
		{
			AddOpenBehavior(openBehaviorType, serviceLifetime);
		}

		return this;
	}

	#endregion

	#region AddBehavior

	public IMediatorConfiguration AddBehavior<TBehavior>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
		where TBehavior : IBasePipelineBehavior
	{
		return AddBehavior(typeof(TBehavior), serviceLifetime);
	}

	public IMediatorConfiguration AddBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		if (implementationType.IsGenericType)
		{
			throw new ArgumentException($"{nameof(implementationType)} cannot be a generic type", nameof(implementationType));
		}

		var implementedInterfaces = implementationType.GetInterfaces()
			.Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)).ToHashSet();

		if (implementedInterfaces.Count == 0)
		{
			throw new ArgumentException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).Name}", nameof(implementationType));
		}

		foreach (var implementedInterface in implementedInterfaces)
		{
			_services.AddDistinctImplementation(implementedInterface, implementationType, serviceLifetime);
		}

		return this;
	}

	#endregion

	#region RegisterServices

	public IMediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
	{
		return RegisterServicesFromAssemblyContaining(typeof(T));
	}

	public IMediatorConfiguration RegisterServicesFromAssemblyContaining(Type markerType)
	{
		return RegisterServicesFromAssembly(markerType.Assembly);
	}

	public IMediatorConfiguration RegisterServicesFromAssemblies(params IEnumerable<Assembly> assemblies)
	{
		foreach (var assembly in assemblies)
		{
			RegisterServicesFromAssembly(assembly);
		}

		return this;
	}

	public IMediatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
	{
		var types = assembly.GetTypes();
		foreach (var type in types)
		{
			TryRegisterType(type);
		}

		return this;
	}

	/// <summary>
	///     Internal method only used to simplify testing
	/// </summary>
	internal MediatorConfiguration RegisterServices(params IEnumerable<Type> types)
	{
		foreach (var type in types)
		{
			TryRegisterType(type);
		}

		return this;
	}

	private void TryRegisterType(Type type)
	{
		if (type.IsAbstract || type.IsInterface)
		{
			return;
		}

		if (!type.IsAssignableTo(typeof(IBaseRequestHandler)) && !type.IsAssignableTo(typeof(IBaseNotificationHandler)))
		{
			return;
		}


		if (type.IsGenericType)
		{
			RegisterGenericType(type);
		}
		else
		{
			RegisterClosedType(type);
		}
	}

	private void RegisterClosedType(Type type)
	{
		var interfaces = type.GetInterfaces();

		var implementedResponseInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
		foreach (var implementedResponseInterface in implementedResponseInterfaces)
		{
			_services.AddDistinctService(implementedResponseInterface, type);
		}

		var implementedNoResponseInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<>));
		foreach (var implementedNoResponseInterface in implementedNoResponseInterfaces)
		{
			_services.AddDistinctService(implementedNoResponseInterface, type);
		}

		var implementedNotificationInterfaces = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
		foreach (var implementedNotificationInterface in implementedNotificationInterfaces)
		{
			_services.AddDistinctImplementation(implementedNotificationInterface, type);
		}
	}

	private void RegisterGenericType(Type type)
	{
		if (type.IsAssignableTo(typeof(IBaseRequestHandler)))
		{
			_services.RegisterGenericRequestHandler(type);
		}
		else // Must be notification
		{
			_services.RegisterGenericNotificationHandler(type);
		}
	}

	#endregion
}