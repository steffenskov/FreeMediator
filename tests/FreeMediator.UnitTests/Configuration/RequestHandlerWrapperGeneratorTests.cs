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
		var type = typeof(FakeResponseandler<>);
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

	public class FakeResponseandler<TResponse> : IRequestHandler<FakeRequest<TResponse>, TResponse>
	{
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