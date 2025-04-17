using System.Reflection;
using FreeMediator.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

			var implementedResponseInterface = interfaces.SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)); // TODO: Support multiple implementations
			if (implementedResponseInterface is not null)
			{
				_services.TryAddTransient(implementedResponseInterface, type);
			}

			var implementedNoResponseInterface = interfaces.SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<>)); // TODO: Support multiple implementations
			if (implementedNoResponseInterface is not null)
			{
				_services.TryAddTransient(implementedNoResponseInterface, type);
			}

			var implementedNotificationInterface = interfaces.SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>)); // TODO: Support multiple implementations
			if (implementedNotificationInterface is not null)
			{
				_services.AddTransientDistinctImplementation(implementedNotificationInterface, type);
			}
		}
	}
}