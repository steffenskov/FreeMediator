namespace FreeMediator.Handlers;

public interface IBaseRequestHandler<TResponse> : IBaseRequestHandler
{
	Task IBaseRequestHandler.Handle(IBaseRequest request, CancellationToken cancellationToken)
	{
		return Handle((IRequest<TResponse>)request, cancellationToken);
	}

	new Task<TResponse> Handle(IBaseRequest request, CancellationToken cancellationToken);
}

public interface IBaseRequestHandler
{
	Task Handle(IBaseRequest request, CancellationToken cancellationToken);
}