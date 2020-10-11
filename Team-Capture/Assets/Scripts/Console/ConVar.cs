using System;
using Voltstro.CommandLineParser;

namespace Console
{
	/// <summary>
	/// An editable variable in both the console and via launch arguments
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ConVar : CommandLineArgumentAttribute
	{
		public ConVar(string name, string summary, string callback = null) : base(name)
		{
			Summary = summary;
			Callback = callback;
		}

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