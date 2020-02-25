using System;

namespace Core.Console
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ConCommand : Attribute
	{
		public ConCommand(string name, string summary, int minArgs = 0, int maxArgs = 0)
		{
			Name = name;
			Summary = summary;
		}

		public string Name { get; set; }

		public string Summary { get; set; }
	}
}
