using System;
using UnityEngine.Scripting;

namespace Console
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConVar : PreserveAttribute
	{
		public ConVar(string name, string summary)
		{
			Name = name;
			Summary = summary;
		}

		public string Name { get; }
		public string Summary { get; }
	}
}