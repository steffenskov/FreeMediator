namespace FreeMediator.ManualHandlerRegistration;

public record GenericRequest<T>(T Value) : IRequest<bool>;

public class NestedGenericHandler<T> : IRequestHandler<GenericRequest<T>, bool>
{
	public async Task<bool> Handle(GenericRequest<T> request, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		return request.Value is not null;
	}
}