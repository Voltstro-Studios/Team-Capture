using System;
using Voltstro.CommandLineParser;

namespace Console
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConVar : CommandLineArgumentAttribute
	{
		public ConVar(string name, string summary) : base(name)
		{
			Summary = summary;
		}

		public string Summary { get; }
	}
}