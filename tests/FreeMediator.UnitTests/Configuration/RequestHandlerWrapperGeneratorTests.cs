using FreeMediator.Exceptions;

namespace FreeMediator.UnitTests.Configuration;

public partial class RequestHandlerWrapperGeneratorTests
{
	[Fact]
	public void GenerateImplementationType_IsPartialRequestHandlerType_WrapsType()
	{
		// Arrange
		var type = typeof(FakeRequestHandler<>);
		var interfaceType = type.GetInterfaces()[0];

		// Act
		var wrapperType = RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType);

		// Assert
		Assert.NotNull(wrapperType);
	}

	[Fact]
	public void GenerateImplementationType_IsPartialResponseHandlerType_WrapsType()
	{
		// Arrange
		var type = typeof(FakeResponseHandler<>);
		var interfaceType = type.GetInterfaces()[0];

		// Act
		var wrapperType = RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType);

		// Assert
		Assert.NotNull(wrapperType);
	}

	[Fact]
	public void GenerateImplementationType_DoesNotImplementProperInterface_Throws()
	{
		// Arrange
		var type = typeof(FakeImproperHandler);
		var interfaceType = type.GetInterfaces()[0];

		// Act && Assert
		var ex = Assert.Throws<UnmappableHandlerException>(() => RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType));

		Assert.Equal($"Cannot wrap type {type.Name} as it doesn't seem to implement IRequestHandler<,>", ex.Message);
	}

	[Fact]
	public void GenerateImplementationType_HasNoGenericArguments_Throws()
	{
		// Arrange
		var type = typeof(FakeImproperHandlerWithCorrectInterface);
		var interfaceType = type.GetInterfaces()[0];

		// Act && Assert
		var ex = Assert.Throws<UnmappableHandlerException>(() => RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType));

		Assert.Equal($"Cannot wrap type {type.Name} as its IRequestHandler definition has no generic type arguments", ex.Message);
	}

	[Fact]
	public void GenerateImplementationType_HasBothGenericArguments_Throws()
	{
		// Arrange
		var type = typeof(FakeImproperHandlerWithBothGenericArguments<,>);
		var interfaceType = type.GetInterfaces()[0];

		// Act && Assert
		var ex = Assert.Throws<UnmappableHandlerException>(() => RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType));

		Assert.Equal($"Cannot wrap type {type.Name} as it already has both generic type arguments", ex.Message);
	}

	[Fact]
	public void GenerateImplementationType_HandlerHasConstructorArguments_WrapsType()
	{
		// Arrange
		var type = typeof(FakeRepositoryResponseHandler<>);
		var interfaceType = type.GetInterfaces()[0];

		// Act
		var wrapperType = RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType);
		var closedType = wrapperType.MakeGenericType(typeof(FakeRequest<string>), typeof(string));
		var instance = Activator.CreateInstance(closedType, new FakeRepository());

		// Assert
		Assert.NotNull(wrapperType);
		Assert.NotNull(instance);
	}

	[Fact]
	public void GenerateImplementationType_HandlerHasManyConstructorArguments_WrapsType()
	{
		// Arrange
		var type = typeof(FakeManyArgResponseHandler<>);
		var interfaceType = type.GetInterfaces()[0];

		// Act
		var wrapperType = RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType);
		var closedType = wrapperType.MakeGenericType(typeof(FakeRequest<string>), typeof(string));
		var instance = Activator.CreateInstance(closedType, new FakeRepository(), "Name", 42, new byte[10], DateTime.UtcNow.Ticks);

		// Assert
		Assert.NotNull(wrapperType);
		Assert.NotNull(instance);
	}

	[Fact]
	public void GenerateImplementationType_HandlerHasMultipleConstructors_Throws()
	{
		// Arrange
		var type = typeof(FakeManyConstructorResponseHandler<>);
		var interfaceType = type.GetInterfaces()[0];

		// Act && Assert
		var ex = Assert.Throws<UnmappableHandlerException>(() => RequestHandlerWrapperGenerator.GenerateImplementationType(type, interfaceType));

		Assert.Equal($"Cannot wrap type {type.Name} as it has multiple constructors.", ex.Message);
	}
}

public partial class RequestHandlerWrapperGeneratorTests
{
	public class FakeRequestHandler<TRequest> : IRequestHandler<TRequest, Unit>
		where TRequest : IRequest<Unit>
	{
		public Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeRequest<TResponse> : IRequest<TResponse>
	{
	}

	public class FakeResponseHandler<TResponse> : IRequestHandler<FakeRequest<TResponse>, TResponse>
	{
		public Task<TResponse> Handle(FakeRequest<TResponse> request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public interface IRepository
	{
	}

	public class FakeRepository : IRepository
	{
	}

	public class FakeRepositoryResponseHandler<TResponse> : IRequestHandler<FakeRequest<TResponse>, TResponse>
	{
		public FakeRepositoryResponseHandler(IRepository repository)
		{
		}

		public Task<TResponse> Handle(FakeRequest<TResponse> request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeManyArgResponseHandler<TResponse> : IRequestHandler<FakeRequest<TResponse>, TResponse>
	{
		public FakeManyArgResponseHandler(IRepository repository, string name, int age, byte[] data, long ticks) // Just need a bunch of args, to roll into the OpCodes.Ldarg_S
		{
		}

		public Task<TResponse> Handle(FakeRequest<TResponse> request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeManyConstructorResponseHandler<TResponse> : IRequestHandler<FakeRequest<TResponse>, TResponse>
	{
		public FakeManyConstructorResponseHandler(IRepository repository)
		{
		}

		public FakeManyConstructorResponseHandler()
		{
		}

		public Task<TResponse> Handle(FakeRequest<TResponse> request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeRequest : IRequest;

	public class FakeImproperHandler : IRequestHandler<FakeRequest>
	{
		public Task Handle(FakeRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeImproperHandlerWithCorrectInterface : IRequestHandler<FakeRequest, Unit>
	{
		public Task<Unit> Handle(FakeRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeImproperHandlerWithBothGenericArguments<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
		where TRequest : FakeRequest<TResponse>
	{
		public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}