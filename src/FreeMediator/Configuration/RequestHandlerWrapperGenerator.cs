using System.Reflection.Emit;

namespace FreeMediator.Configuration;

static internal class RequestHandlerWrapperGenerator
{
	private static readonly ModuleBuilder _moduleBuilder;

	static RequestHandlerWrapperGenerator()
	{
		var aName = new AssemblyName($"RequestHandlerWrapperGeneratorTypes{GenerateStrippedGuid()}");
		var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

		// The module name is usually the same as the assembly name.
		_moduleBuilder = ab.DefineDynamicModule(aName.Name!);
	}

	/// <summary>
	///     Generates a wrapper class for the request handler and returns an instance of it.
	/// </summary>
	public static Type GenerateImplementationType(Type type)
	{
		var tb = _moduleBuilder.DefineType($"{type.Name}Wrapper{GenerateStrippedGuid()}", TypeAttributes.NotPublic);

		var baseGenericType = type.GetGenericTypeDefinition();

		var genericParams = tb.DefineGenericParameters("TRequest", "TResponse");
		var tRequest = genericParams[0];

		tRequest.SetInterfaceConstraints(baseGenericType.GetGenericArguments()[0].GetGenericParameterConstraints());

		var baseType = baseGenericType.MakeGenericType(tRequest);
		tb.SetParent(baseType);

		return tb.CreateType();
	}

	private static string GenerateStrippedGuid()
	{
		return Guid.NewGuid().ToString().Replace("-", string.Empty);
	}
}