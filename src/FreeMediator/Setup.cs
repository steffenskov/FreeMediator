using FreeMediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FreeMediator;

public static class Setup
{
	public static void AddMediator(this IServiceCollection services, Action<MediatorConfiguration> configure)
	{
		services.TryAddTransient<IMediator, Mediator>();
		services.TryAddTransient<ISender, Mediator>();
		services.TryAddTransient<IPublisher, Mediator>();

		var configuration = new MediatorConfiguration(services);
		configure(configuration);
	}
}