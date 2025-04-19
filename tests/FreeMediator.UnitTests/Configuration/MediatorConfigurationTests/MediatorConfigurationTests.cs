namespace FreeMediator.UnitTests.Configuration.MediatorConfigurationTests;

public partial class MediatorConfigurationTests
{
	private (MediatorConfiguration, ServiceCollection) CreateConfiguration(bool useForgivingRegistrar = false)
	{
		var services = new ServiceCollection();
		IServiceRegistrar registrar = useForgivingRegistrar
			? new ForgivingServiceRegistrar(services)
			: new ServiceRegistrar(services);

		return (new MediatorConfiguration(registrar), services);
	}
}