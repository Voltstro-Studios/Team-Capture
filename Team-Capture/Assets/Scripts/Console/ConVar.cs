using System;

namespace Console
{
	/// <summary>
	/// An editable variable in both the console and via launch arguments
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ConVar : Attribute
	{
		public ConVar(string name, string summary, string callback = null)
		{
			Name = name;
			Summary = summary;
			Callback = callback;
		}

		/// <summary>
		/// The name of this ConVar
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The summary of this ConVar
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The function witch will be called when the value gets updated
		/// </summary>
		public string Callback { get; }
	}
}