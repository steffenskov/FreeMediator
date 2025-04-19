namespace FreeMediator.UnitTests.Services;

public class PublisherTests
{
	private readonly IPublisher _publisher;

	public PublisherTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config => { });

		var config = new MediatorConfiguration(new ForgivingServiceRegistrar(services));
		config.RegisterServicesFromAssemblyContaining<PublisherTests>();

		var serviceProvider = services.BuildServiceProvider();
		_publisher = serviceProvider.GetRequiredService<IPublisher>();
	}

	[Fact]
	public async Task Publish_MultipleHandlers_AllAreInvoked()
	{
		// Arrange
		var notification = new MultiRecipientNotification($"Hello world {Random.Shared.Next()}");

		// Act
		await _publisher.Publish(notification);

		// Assert
		var firstHandledMessage = Assert.Single(FirstMultiRecipientHandler.HandledMessages);
		var secondHandledMessage = Assert.Single(SecondMultiRecipientHandler.HandledMessages);
		var thirdHandledMessage = Assert.Single(MultipleImplementationsHandler.HandledMessages);
		Assert.Equal(notification.Message, firstHandledMessage);
		Assert.Equal(notification.Message, secondHandledMessage);
		Assert.Equal(notification.Message, thirdHandledMessage);

		// Cleanup
		FirstMultiRecipientHandler.HandledMessages.Clear();
		SecondMultiRecipientHandler.HandledMessages.Clear();
		MultipleImplementationsHandler.HandledMessages.Clear();
	}

	[Fact]
	public async Task Publish_NoHandlers_DoesNothing()
	{
		// Arrange
		var notification = new NoRecipientNotification($"Hello world {Random.Shared.Next()}");

		// Act
		var ex = await Record.ExceptionAsync(async () => await _publisher.Publish(notification));

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public async Task Publish_MultipleHandlersOneThrows_ThrowsButInvokesAll()
	{
		// Arrange
		var notification = new MultiRecipientWithExceptionNotification($"Hello world {Random.Shared.Next()}");

		// Act && Assert
		var ex = await Assert.ThrowsAsync<AggregateException>(async () => await _publisher.Publish(notification));

		var innerEx = Assert.Single(ex.InnerExceptions);
		var handledMessage = Assert.Single(WorkingMultiRecipientHandler.HandledMessages);

		Assert.Equal(notification.Message, handledMessage);
		Assert.IsType<InvalidOperationException>(innerEx);
		Assert.Equal("An error occurred while handling the notification.", innerEx.Message);

		// Cleanup
		FirstMultiRecipientHandler.HandledMessages.Clear();
		SecondMultiRecipientHandler.HandledMessages.Clear();
		MultipleImplementationsHandler.HandledMessages.Clear();
	}

	[Fact]
	public async Task Publish_GenericRequest_IsHandled()
	{
		// Arrange
		var intNotification = new GenericNotification<int>(42);
		var stringNotification = new GenericNotification<string>($"Hello world {Random.Shared.Next()}");

		// Act
		await _publisher.Publish(intNotification);
		await _publisher.Publish(stringNotification);

		// Assert
		var intMessage = Assert.Single(GenericNotificationHandler<GenericNotification<int>>.HandledMessages);
		var stringMessage = Assert.Single(GenericNotificationHandler<GenericNotification<string>>.HandledMessages);

		Assert.Equal(intNotification.Value, intMessage);
		Assert.Equal(stringNotification.Value, stringMessage);
	}
}

file record MultiRecipientNotification(string Message) : INotification;

file record NoRecipientNotification(string Message) : INotification;

file record MultiRecipientWithExceptionNotification(string Message) : INotification;

file class FirstMultiRecipientHandler : INotificationHandler<MultiRecipientNotification>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(MultiRecipientNotification notification, CancellationToken cancellationToken)
	{
		HandledMessages.Add(notification.Message);
		return Task.CompletedTask;
	}
}

file class SecondMultiRecipientHandler : INotificationHandler<MultiRecipientNotification>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(MultiRecipientNotification notification, CancellationToken cancellationToken)
	{
		HandledMessages.Add(notification.Message);
		return Task.CompletedTask;
	}
}

file class ExceptionThrowingHandler : INotificationHandler<MultiRecipientWithExceptionNotification>
{
	public Task Handle(MultiRecipientWithExceptionNotification notification, CancellationToken cancellationToken)
	{
		throw new InvalidOperationException("An error occurred while handling the notification.");
	}
}

file class WorkingMultiRecipientHandler : INotificationHandler<MultiRecipientWithExceptionNotification>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(MultiRecipientWithExceptionNotification notification, CancellationToken cancellationToken)
	{
		HandledMessages.Add(notification.Message);
		return Task.CompletedTask;
	}
}

file class MultipleImplementationsHandler : INotificationHandler<MultiRecipientNotification>, INotificationHandler<MultiRecipientWithExceptionNotification>
{
	public static List<string> HandledMessages { get; } = [];

	public Task Handle(MultiRecipientNotification notification, CancellationToken cancellationToken)
	{
		HandledMessages.Add(notification.Message);
		return Task.CompletedTask;
	}

	public Task Handle(MultiRecipientWithExceptionNotification notification, CancellationToken cancellationToken)
	{
		HandledMessages.Add(notification.Message);
		return Task.CompletedTask;
	}
}

file record GenericNotification<T>(T Value) : IGenericNotification
{
	object? IGenericNotification.Value => Value;
}

file interface IGenericNotification : INotification
{
	object? Value { get; }
}

file class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
	where TNotification : IGenericNotification
{
	public static List<object?> HandledMessages { get; } = [];

	public Task Handle(TNotification notification, CancellationToken cancellationToken = default)
	{
		HandledMessages.Add(notification.Value);
		return Task.CompletedTask;
	}
}