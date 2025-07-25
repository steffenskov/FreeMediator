using System.Reflection.Emit;
using FreeMediator.Exceptions;

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
	public static Type GenerateImplementationType(Type type, Type genericInterfaceType)
	{
		var tb = _moduleBuilder.DefineType($"{type.Name}Wrapper{GenerateStrippedGuid()}", TypeAttributes.NotPublic);

		var genericInterfaceArguments = genericInterfaceType.GetGenericArguments();
		if (genericInterfaceArguments.Length != 2)
		{
			throw new InvalidOperationException($"Cannot wrap type {type.Name} as it doesn't seem to implement IRequestHandler<,>");
		}

		var isGenericRequest = genericInterfaceArguments[0].IsGenericParameter;
		var isGenericResponse = genericInterfaceArguments[1].IsGenericParameter;

		if (isGenericRequest && isGenericResponse)
		{
			throw new InvalidOperationException($"Cannot wrap type {type.Name} as it already has both generic type arguments");
		}

		if (!isGenericRequest && !isGenericResponse)
		{
			throw new UnmappableHandlerException(type);
		}

		var baseGenericType = type.GetGenericTypeDefinition();

		var genericParams = tb.DefineGenericParameters("TRequest", "TResponse");

		Type baseType;
		if (isGenericRequest)
		{
			var tRequest = genericParams[0];

			tRequest.SetInterfaceConstraints(baseGenericType.GetGenericArguments()[0].GetGenericParameterConstraints());

			baseType = baseGenericType.MakeGenericType(tRequest);
		}
		else // Must be generic Response, due to the sanity checks above
		{
			var tResponse = genericParams[1];

			tResponse.SetInterfaceConstraints(baseGenericType.GetGenericArguments()[0].GetGenericParameterConstraints());

			baseType = baseGenericType.MakeGenericType(tResponse);
		}

		tb.SetParent(baseType);

		return tb.CreateType();
	}

	private static string GenerateStrippedGuid()
	{
		return Guid.NewGuid().ToString().Replace("-", string.Empty);
	}
}