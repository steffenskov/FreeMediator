using FreeMediator.Handlers;

// ReSharper disable once CheckNamespace
namespace FreeMediator;

#region With response

public interface IRequestHandler<in TRequest, TResponse> : IBaseRequestHandler<TResponse>
	where TRequest : IRequest<TResponse>
{
	Task<TResponse> IBaseRequestHandler<TResponse>.Handle(IBaseRequest request, CancellationToken cancellationToken)
	{
		return Handle((TRequest)request, cancellationToken);
	}

	Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

#endregion

#region No response

public interface IRequestHandler<in TRequest> : IBaseRequestHandler
	where TRequest : IRequest
{
	Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

#endregion