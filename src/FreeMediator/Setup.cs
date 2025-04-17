using FreeMediator.Configuration;
using FreeMediator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FreeMediator;

public static class Setup
{
	public static void AddMediator(this IServiceCollection services, Action<MediatorConfiguration> configure)
	{
		services.TryAddTransient<IMediator, Mediator>();

		var configuration = new MediatorConfiguration(services);
		configure(configuration);
	}
}