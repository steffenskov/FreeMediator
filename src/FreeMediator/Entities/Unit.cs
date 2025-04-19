namespace FreeMediator;

/// <summary>
///     Represents an empty response.
/// </summary>
public struct Unit
{
	private static readonly Unit _instance = new();

	public static Task<Unit> Task => System.Threading.Tasks.Task.FromResult(_instance);

	public Unit Value => _instance;
}