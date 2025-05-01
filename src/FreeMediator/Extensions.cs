namespace FreeMediator;

static internal class Extensions
{
	public static GenericInterfaceType GetRequestHandlerInterface(this Type type)
	{
		return type
			.GetInterfaces()
			.Where(t => t.IsGenericType)
			.Select(t =>
				new GenericInterfaceType(t, t.GetGenericTypeDefinition())
			)
			.Single(t =>
				t.GenericTypeDefinition == typeof(IRequestHandler<>) || t.GenericTypeDefinition == typeof(IRequestHandler<,>)
			);
	}
}

internal record GenericInterfaceType(Type InterfaceType, Type GenericTypeDefinition);