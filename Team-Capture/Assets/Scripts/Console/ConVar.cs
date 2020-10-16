using System;

namespace Console
{
	/// <summary>
	/// An editable variable in both the console
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ConVar : Attribute
	{
		public ConVar(string name, string summary)
		{
			Name = name;
			Summary = summary;
		}

		public ConVar(string name, string summary, string callback)
		{
			Name = name;
			Summary = summary;
			Callback = callback;
		}

		public ConVar(string name, string summary, string callback, bool graphicsOnly)
		{
			Name = name;
			Summary = summary;
			Callback = callback;
			GraphicsOnly = graphicsOnly;
		}

		public ConVar(string name, string summary, bool graphicsOnly)
		{
			Name = name;
			Summary = summary;
			Callback = null;
			GraphicsOnly = graphicsOnly;
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

		/// <summary>
		/// This ConVar can only be used in graphics mode
		/// </summary>
		public bool GraphicsOnly { get; }
	}
}