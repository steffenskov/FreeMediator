using Microsoft.Extensions.DependencyInjection;

namespace FreeMediator.UnitTests.Services;

public class PublisherTests
{
	private readonly IPublisher _publisher;

	public PublisherTests()
	{
		var services = new ServiceCollection();
		services.AddMediator(config => { config.RegisterServicesFromAssemblyContaining<PublisherTests>(); });

		var serviceProvider = services.BuildServiceProvider();
		_publisher = serviceProvider.GetRequiredService<IPublisher>();
	}

	[Fact]
	public async Task Publish_MultipleHandlers_AllAreInvoked()
	{
		// Arrange
		var notification = new MultiRecipientNotification($"Hello world {Random.Shared.Next()}");

		// Act
		await _publisher.Publish(notification, TestContext.Current.CancellationToken);

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
	public async Task Publish_MultipleRegistrations_HandlersOnlyRegisteredOnce()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddMediator(config =>
		{
			config.RegisterServicesFromAssemblyContaining<PublisherTests>();
			config.RegisterServicesFromAssemblyContaining<PublisherTests>();
		});

		var serviceProvider = services.BuildServiceProvider();
		var publisher = serviceProvider.GetRequiredService<IPublisher>();

		var notification = new MultiRecipientNotification($"Hello world {Random.Shared.Next()}");

		// Act
		await publisher.Publish(notification, TestContext.Current.CancellationToken);

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
		var ex = await Record.ExceptionAsync(async () => await _publisher.Publish(notification, TestContext.Current.CancellationToken));

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public async Task Publish_MultipleHandlersOneThrows_ThrowsButInvokesAll()
	{
		// Arrange
		var notification = new MultiRecipientWithExceptionNotification($"Hello world {Random.Shared.Next()}");

		// Act && Assert
		var ex = await Assert.ThrowsAsync<AggregateException>(async () => await _publisher.Publish(notification, TestContext.Current.CancellationToken));

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