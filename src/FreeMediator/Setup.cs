namespace FreeMediator;

public static class Setup
{
	public static void AddMediator(this IServiceCollection services, Action<MediatorConfiguration> configure)
	{
		services.TryAddTransient<IMediator, Mediator>();
		services.TryAddTransient<ISender>(provider => provider.GetRequiredService<IMediator>());
		services.TryAddTransient<IPublisher>(provider => provider.GetRequiredService<IMediator>());

		var configuration = new MediatorConfiguration(new ServiceRegistrar(services));
		configure(configuration);
	}
}