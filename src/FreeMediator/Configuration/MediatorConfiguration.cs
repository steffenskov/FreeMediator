namespace FreeMediator.Configuration;

public class MediatorConfiguration
{
	private readonly IServiceCollection _services;

	internal MediatorConfiguration(IServiceCollection services)
	{
		_services = services;
	}

	#region AddOpenBehavior

	public MediatorConfiguration AddOpenBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		if (!implementationType.IsGenericType || !implementationType.IsGenericTypeDefinition)
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
			throw new ArgumentException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}", nameof(implementationType));
		}

		_services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), implementationType, serviceLifetime));

		return this;
	}

	public MediatorConfiguration AddOpenBehaviors(IEnumerable<Type> openBehaviorTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		foreach (var openBehaviorType in openBehaviorTypes)
		{
			AddOpenBehavior(openBehaviorType, serviceLifetime);
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
		if (implementationType.IsGenericType)
		{
			throw new ArgumentException($"{nameof(implementationType)} cannot be a generic type", nameof(implementationType));
		}

		var implementedInterfaces = implementationType.GetInterfaces()
			.Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)).ToHashSet();

		if (implementedInterfaces.Count == 0)
		{
			throw new ArgumentException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}", nameof(implementationType));
		}

		foreach (var implementedInterface in implementedInterfaces)
		{
			_services.Add(new ServiceDescriptor(implementedInterface, implementationType, serviceLifetime));
		}

		return this;
	}

	#endregion

	#region RegisterServices

	public MediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
	{
		return RegisterServicesFromAssemblyContaining(typeof(T));
	}

	public MediatorConfiguration RegisterServicesFromAssemblyContaining(Type markerType)
	{
		return RegisterServicesFromAssembly(markerType.Assembly);
	}

	public MediatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
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

	public MediatorConfiguration RegisterServicesFromAssemblies(params IEnumerable<Assembly> assemblies)
	{
		foreach (var assembly in assemblies)
		{
			RegisterServicesFromAssembly(assembly);
		}

		return this;
	}

	#endregion
}