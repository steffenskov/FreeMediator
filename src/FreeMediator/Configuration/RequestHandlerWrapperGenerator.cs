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
			throw new UnmappableHandlerException($"Cannot wrap type {type.Name} as it doesn't seem to implement IRequestHandler<,>");
		}

		var isGenericRequest = genericInterfaceArguments[0].IsGenericParameter;
		var isGenericResponse = genericInterfaceArguments[1].IsGenericParameter;

		if (isGenericRequest && isGenericResponse)
		{
			throw new UnmappableHandlerException($"Cannot wrap type {type.Name} as it already has both generic type arguments");
		}

		if (!isGenericRequest && !isGenericResponse)
		{
			throw new UnmappableHandlerException($"Cannot wrap type {type.Name} as its IRequestHandler definition has no generic type arguments");
		}

		GenerateConstructor(type, tb);

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

	private static void GenerateConstructor(Type type, TypeBuilder tb)
	{
		var constructor = GetConstructor(type);

		var constructorArgs = constructor.GetParameters()
			.Select(p => p.ParameterType)
			.ToArray();
		if (constructorArgs.Length == 0)
		{
			return;
		}

		var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorArgs);
		var il = ctorBuilder.GetILGenerator();

		// Load 'this' (arg 0)
		il.Emit(OpCodes.Ldarg_0);

		for (var i = 1; i <= constructorArgs.Length; i++)
		{
			if (i <= 3)
			{
				il.Emit(OpCodes.Ldarg, i);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_S, i);
			}
		}

		// Call base constructor
		il.Emit(OpCodes.Call, constructor);

		// Return from constructor
		il.Emit(OpCodes.Ret);
	}

	private static ConstructorInfo GetConstructor(Type type)
	{
		var ctorSignatures = type.GetConstructors();
		if (ctorSignatures.Length > 1)
		{
			throw new UnmappableHandlerException($"Cannot wrap type {type.Name} as it has multiple constructors.");
		}

		var ctorSignature = ctorSignatures[0];

		return ctorSignature;
	}

	private static string GenerateStrippedGuid()
	{
		return Guid.NewGuid().ToString().Replace("-", string.Empty);
	}
}