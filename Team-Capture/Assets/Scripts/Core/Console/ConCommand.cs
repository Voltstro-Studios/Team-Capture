using System;

namespace Core.Console
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ConCommand : Attribute
	{
		public string Name { get; set; }

		public string Summary { get; set; }
	}
}
