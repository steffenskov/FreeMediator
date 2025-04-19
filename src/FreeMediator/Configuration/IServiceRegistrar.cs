namespace FreeMediator.Configuration;

internal interface IServiceRegistrar : IEnumerable<ServiceDescriptor>
{
	void AddDistinctImplementation(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient);
	void AddDistinctService(Type service, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient);
	void RegisterGenericRequestHandler(Type type);
	void RegisterGenericNotificationHandler(Type type);
}