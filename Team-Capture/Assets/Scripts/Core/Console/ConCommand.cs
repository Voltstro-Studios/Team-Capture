using System;

namespace Core.Console
{
	/// <summary>
	/// Marks a method to be used as a command for the in-game console.
	/// <para>METHOD MUST BE STATIC</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ConCommand : Attribute
	{
		/// <summary>
		/// Marks a method to be used as a command for the in-game console.
		/// <para>METHOD MUST BE STATIC</para>
		/// </summary>
		/// <param name="name">The command</param>
		/// <param name="summary">A quick summary on what the command does</param>
		/// <param name="minArgs">Min amount of args required (optional)</param>
		/// <param name="maxArgs">Max amount of args required (optional)</param>
		public ConCommand(string name, string summary, int minArgs = 0, int maxArgs = 0)
		{
			Name = name;
			Summary = summary;

			if (minArgs > maxArgs)
			{
				Logger.Logger.Log(
					$"Min args cannot be less then max args! Argument requirements have not been set for the command `{name}`.");
				return;
			}

			MinArguments = minArgs;
			MaxArguments = maxArgs;
		}

		/// <summary>
		/// The name of this command
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Summary of the command
		/// </summary>
		public string Summary { get; set; }

		/// <summary>
		/// Min amount of args required for the command to work
		/// </summary>
		public int MinArguments { get; set; }

		/// <summary>
		/// Max amount of args required for the command to work
		/// </summary>
		public int MaxArguments { get; set; }
	}
}