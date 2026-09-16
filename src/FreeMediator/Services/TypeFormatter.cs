namespace FreeMediator;

static internal class TypeFormatter
{
	public static string FormatTypeName(Type? type)
	{
		if (type is null)
		{
			return "";
		}

		var baseName = type.FullName ?? type.Name;
		if (!type.IsGenericType)
		{
			return baseName;
		}


		var indexOfGenericDelimiter = baseName.IndexOf('`', StringComparison.Ordinal);
		baseName = baseName.Remove(indexOfGenericDelimiter);

		var genericArgs = type.GetGenericArguments();

		if (type.IsGenericTypeDefinition)
		{
			return $"{baseName}<{string.Join(",", genericArgs.Select(_ => ""))}>"; // Empty generic args and no spacing
		}

		return $"{baseName}<{string.Join(", ", genericArgs.Select(FormatTypeName))}>";
	}
}