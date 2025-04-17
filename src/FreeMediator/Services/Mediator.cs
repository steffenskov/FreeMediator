using FreeMediator.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FreeMediator.Services;

internal class Mediator : IMediator
{
	private readonly IServiceProvider _serviceProvider;

	public Mediator(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

		var service = (IBaseRequestHandler<TResponse>)_serviceProvider.GetRequiredService(handlerType);

		return await service.Handle(request, cancellationToken);
	}

	public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
	{
		ArgumentNullException.ThrowIfNull(request);

		var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());

		var service = (IBaseRequestHandler)_serviceProvider.GetRequiredService(handlerType);

		await service.Handle(request, cancellationToken);
	}
}