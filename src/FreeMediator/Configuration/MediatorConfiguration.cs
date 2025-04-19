using System.Diagnostics;

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

	public MediatorConfiguration RegisterServicesFromAssemblies(params IEnumerable<Assembly> assemblies)
	{
		foreach (var assembly in assemblies)
		{
			RegisterServicesFromAssembly(assembly);
		}

		return this;
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

			var interfaces = type.GetInterfaces();

			if (type.IsGenericType)
			{
				if (type.IsAssignableTo(typeof(IBaseRequestHandler)))
				{
					RegisterGenericRequestHandler(type);
				}
				else // Must be notification
				{
					RegisterGenericNotificationHandler(type);
				}
			}

			else
			{
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
		}

		return this;
	}

	private void RegisterGenericRequestHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 0: throw new UnreachableException($"Generic type must have at least one argument: {type.FullName}");
			case 1: // TODO: Wrap in handler with 2 args
				throw new NotImplementedException($"Generic request handlers with a single generic type argument is not yet supported: {type.FullName}");
			case 2:
				_services.TryAddTransient(typeof(IRequestHandler<,>), type);
				break;
			default: throw new NotSupportedException($"Generic request handlers with more than 2 generic type arguments are not supported: {type.FullName}");
		}
	}

	private void RegisterGenericNotificationHandler(Type type)
	{
		var genericArgumentTypeCount = type.GetGenericArguments().Length;
		switch (genericArgumentTypeCount)
		{
			case 0: throw new UnreachableException($"Generic type must have at least one argument: {type.FullName}");
			case 1:
				_services.TryAddTransient(typeof(INotificationHandler<>), type);
				break;
			default: throw new NotSupportedException($"Generic notification handlers with more than 1 generic type arguments are not supported: {type.FullName}");
		}
	}

	#endregion
}