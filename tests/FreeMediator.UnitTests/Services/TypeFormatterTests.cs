namespace FreeMediator.UnitTests.Services;

public class TypeFormatterTests
{
	[Fact]
	public void FormatTypeName_WithTypeIsNullException_ReturnsEmptyString()
	{
		// Arrange
		Type? type = null;

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("", result);
	}

	[Fact]
	public void FormatTypeName_SimpleType_ReturnsFullName()
	{
		// Arrange
		var type = typeof(string);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.String", result);
	}

	[Fact]
	public void FormatTypeName_GenericType_WithOneArgument_ReturnsFormattedString()
	{
		// Arrange
		var type = typeof(List<string>);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.Collections.Generic.List<System.String>", result);
	}

	[Fact]
	public void FormatTypeName_GenericType_WithMultipleArguments_ReturnsFormattedString()
	{
		// Arrange
		var type = typeof(Dictionary<string, int>);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.Collections.Generic.Dictionary<System.String, System.Int32>", result);
	}

	[Fact]
	public void FormatTypeName_NestedGenericType_ReturnsFormattedString()
	{
		// Arrange
		var type = typeof(List<List<string>>);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.Collections.Generic.List<System.Collections.Generic.List<System.String>>", result);
	}

	[Fact]
	public void FormatTypeName_SystemType_ReturnsFullName()
	{
		// Arrange
		var type = typeof(Type);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.Type", result);
	}

	[Fact]
	public void FormatTypeName_GenericTypeDefinition_ReturnsWithoutGenericArguments()
	{
		// Arrange
		var type = typeof(Dictionary<,>);

		// Act
		var result = TypeFormatter.FormatTypeName(type);

		// Assert
		Assert.Equal("System.Collections.Generic.Dictionary<,>", result);
	}
}