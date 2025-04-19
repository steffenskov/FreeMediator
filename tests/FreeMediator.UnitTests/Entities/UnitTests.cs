namespace FreeMediator.UnitTests.Entities;

public class UnitTests
{
	[Fact]
	public void Unit_Instance_IsSingleton()
	{
		// Arrange & Act
		var instance1 = new Unit();
		var instance2 = new Unit();

		// Assert
		Assert.Equal(instance1, instance2);
	}

	[Fact]
	public async Task Unit_Task_ReturnsCompletedTask()
	{
		// Act
		var unitTask = Unit.Task;

		// Assert
		Assert.NotNull(unitTask);
		Assert.True(unitTask.IsCompletedSuccessfully);
		Assert.Equal(default, await unitTask);
	}

	[Fact]
	public void Unit_Value_ReturnsSingletonInstance()
	{
		// Act
		var unitValue = Unit.Value;

		// Assert
		Assert.Equal(default, unitValue);
	}
}