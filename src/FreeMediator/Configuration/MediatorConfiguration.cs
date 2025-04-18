namespace FreeMediator.Configuration;

public class MediatorConfiguration
{
	private readonly IServiceCollection _services;

	internal MediatorConfiguration(IServiceCollection services)
	{
		_services = services;
	}

	public void RegisterServicesFromAssemblyContaining<T>()
	{
		RegisterServicesFromAssemblyContaining(typeof(T));
	}

	public void RegisterServicesFromAssemblyContaining(Type markerType)
	{
		RegisterServicesFromAssemblyContaining(markerType.Assembly);
	}

	public void RegisterServicesFromAssemblyContaining(Assembly assembly)
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
	}
}