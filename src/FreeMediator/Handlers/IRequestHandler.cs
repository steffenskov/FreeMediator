// ReSharper disable once CheckNamespace

namespace FreeMediator;

#region With response

/// <summary>
///     Implement a handler for requests of type <typeparamref name="TRequest" /> returning
///     <typeparamref name="TResponse" />.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse> : IBaseRequestHandler<TResponse>
	where TRequest : IRequest<TResponse>
{
	Task<TResponse> IBaseRequestHandler<TResponse>.Handle(IBaseRequest request, CancellationToken cancellationToken)
	{
		return Handle((TRequest)request, cancellationToken);
	}

	/// <summary>
	///     Handler logic for the request. Any exceptions thrown will not be caught by the framework.
	///     <param name="request">Request being sent through the mediator</param>
	///     <param name="cancellationToken">CancellationToken</param>
	/// </summary>
	Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

#endregion

#region No response

/// <summary>
///     Implement a handler for requests of type <typeparamref name="TRequest" /> returning no response.
/// </summary>
public interface IRequestHandler<in TRequest> : IBaseRequestHandler
	where TRequest : IRequest
{
	/// <summary>
	///     Handler logic for the request. Any exceptions thrown will not be caught by the framework.
	///     Must return <see cref="Unit" /> to represent the lack of return value.
	///     <param name="request">Request being sent through the mediator</param>
	///     <param name="cancellationToken">CancellationToken</param>
	/// </summary>
	Task<Unit> Handle(TRequest request, CancellationToken cancellationToken);
}

#endregion