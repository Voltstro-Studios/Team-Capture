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
		public ConVar(string name, string summary) : base(name)
		{
			Summary = summary;
		}

		/// <summary>
		/// The summary of this ConVar
		/// </summary>
		public string Summary { get; }
	}
}