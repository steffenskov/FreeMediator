using Microsoft.Extensions.DependencyInjection;

namespace FreeMediator;

internal static class Extensions
{
	public static void AddTransientDistinctImplementation(this IServiceCollection collection, Type service, Type implementationType)
	{
		var descriptor = ServiceDescriptor.Transient(service, implementationType);

		if (collection.Any(registeredDescriptor => registeredDescriptor.ServiceType == descriptor.ServiceType
		                                           && registeredDescriptor.ImplementationType == descriptor.ImplementationType
		                                           && Equals(registeredDescriptor.ServiceKey, descriptor.ServiceKey)))
		{
			return;
		}

		collection.AddTransient(service, implementationType);
	}
}