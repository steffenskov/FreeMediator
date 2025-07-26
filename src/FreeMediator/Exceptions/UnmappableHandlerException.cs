namespace FreeMediator.Exceptions;

/// <summary>
///     Exception throw for handlers that cannot be automatically mapped.
/// </summary>
public class UnmappableHandlerException : Exception
{
	public UnmappableHandlerException(string message) : base(message)
	{
	}
}