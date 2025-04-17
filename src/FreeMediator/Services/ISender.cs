namespace FreeMediator;

/// <summary>
///     Used to send a request to a single handler.
/// </summary>
public interface ISender
{
	/// <summary>
	///     Sends a request to a single handler and returns the response.
	/// </summary>
	/// <param name="request">Request to send</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	/// <returns>Response from handling the request</returns>
	Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

	/// <summary>
	///     Sends a request to a single handler, does not return a response.
	/// </summary>
	/// <param name="request">Request to send</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;
}