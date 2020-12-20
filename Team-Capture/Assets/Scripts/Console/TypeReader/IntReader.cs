using System.Globalization;

namespace Team_Capture.Console.TypeReader
{
	/// <summary>
	///     A default reader for <see cref="int" />
	/// </summary>
	internal sealed class IntReader : ITypeReader
	{
		public object ReadType(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return 0;

			return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : 0;
		}
	}
}