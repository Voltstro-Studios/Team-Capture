namespace Console.TypeReader
{
	/// <summary>
	///     A default reader for <see cref="string" />
	/// </summary>
	internal sealed class StringReader : ITypeReader
	{
		public object ReadType(string input)
		{
			return input;
		}
	}
}