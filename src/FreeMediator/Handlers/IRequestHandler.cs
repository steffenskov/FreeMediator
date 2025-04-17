namespace FreeMediator.Handlers;

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

public interface IBaseRequestHandler<TResponse> : IBaseRequestHandler
{
	Task IBaseRequestHandler.Handle(IBaseRequest request, CancellationToken cancellationToken)
	{
		return Handle((IRequest<TResponse>)request, cancellationToken);
	}

	new Task<TResponse> Handle(IBaseRequest request, CancellationToken cancellationToken);
}

#endregion

#region No response

public interface IRequestHandler<in TRequest> : IBaseRequestHandler
	where TRequest : IRequest
{
	Task IBaseRequestHandler.Handle(IBaseRequest request, CancellationToken cancellationToken)
	{
		return Handle((TRequest)request, cancellationToken);
	}

	Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

#endregion

public interface IBaseRequestHandler
{
	Task Handle(IBaseRequest request, CancellationToken cancellationToken);
}