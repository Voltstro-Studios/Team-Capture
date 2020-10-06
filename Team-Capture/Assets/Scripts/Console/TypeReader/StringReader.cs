namespace Console.TypeReader
{
	/// <summary>
	/// A default reader for <see cref="string"/>
	/// </summary>
	public sealed class StringReader : Console.TypeReader.ITypeReader
	{
		public object ReadType(string input)
		{
			return input;
		}
	}
}