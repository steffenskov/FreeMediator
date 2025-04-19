namespace FreeMediator;

/// <summary>
///     Represents an empty response.
/// </summary>
public struct Unit
{
	public static Task<Unit> Task => System.Threading.Tasks.Task.FromResult(Value);

	public static Unit Value { get; } = new();
}