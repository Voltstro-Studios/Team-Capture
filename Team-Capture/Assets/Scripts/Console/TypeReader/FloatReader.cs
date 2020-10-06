using System.Globalization;

namespace Console.TypeReader
{
	/// <summary>
	/// A default reader for <see cref="float"/>
	/// </summary>
	public sealed class FloatReader : Console.TypeReader.ITypeReader
	{
		public object ReadType(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return 0f;

			return float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)
				? result
				: 0f;
		}
	}
}