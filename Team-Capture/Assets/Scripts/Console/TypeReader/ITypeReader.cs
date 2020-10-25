namespace Console.TypeReader
{
	/// <summary>
	/// The interface for a type reader
	/// </summary>
	internal interface ITypeReader
	{
		/// <summary>
		/// Read the type and return it
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		object ReadType(string input);
	}
}