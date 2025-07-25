namespace FreeMediator.Exceptions;

/// <summary>
///     Exception throw for handlers that cannot be automatically mapped.
/// </summary>
public class UnmappableHandlerException : Exception
{
	public UnmappableHandlerException(Type type) : base($"Cannot wrap type {type.Name} as its IRequestHandler definition has no generic type arguments")
	{
	}
}