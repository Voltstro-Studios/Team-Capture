// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Logging;
using UnityEngine.Scripting;

namespace Team_Capture.Console
{
	/// <summary>
	///     Marks a method to be used as a command for the in-game console.
	///     <para>METHOD MUST BE STATIC</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ConCommand : PreserveAttribute
	{
		/// <summary>
		///     Marks a method to be used as a command for the in-game console.
		///     <para>METHOD MUST BE STATIC</para>
		/// </summary>
		/// <param name="name">The command</param>
		/// <param name="summary">A quick summary on what the command does</param>
		/// <param name="runPermission">What this command can and cannot run on</param>
		/// <param name="minArgs">Min amount of args required (optional)</param>
		/// <param name="maxArgs">Max amount of args required (optional)</param>
		/// <param name="graphicsModeOnly">Can this command only be used in graphics mode</param>
		public ConCommand(string name, string summary, CommandRunPermission runPermission = CommandRunPermission.Both,
			int minArgs = 0, int maxArgs = 0, bool graphicsModeOnly = false)
		{
			Name = name;
			Summary = summary;

			if (minArgs > maxArgs)
			{
				Logger.Error(
					"Min args cannot be less then max args! Argument requirements have not been set for the command `{@Name}`.",
					name);
				return;
			}

			MinArguments = minArgs;
			MaxArguments = maxArgs;
			RunPermission = runPermission;
			GraphicsModeOnly = graphicsModeOnly;
		}

		/// <summary>
		///     The name of this command
		/// </summary>
		public string Name { get; }

		/// <summary>
		///     Summary of the command
		/// </summary>
		public string Summary { get; }

		/// <summary>
		///     Min amount of args required for the command to work
		/// </summary>
		public int MinArguments { get; }

		/// <summary>
		///     Max amount of args required for the command to work
		/// </summary>
		public int MaxArguments { get; }

		/// <summary>
		///     What this command can and cannot run on
		/// </summary>
		public CommandRunPermission RunPermission { get; }

		/// <summary>
		///     This command can only run in a graphics mode
		/// </summary>
		public bool GraphicsModeOnly { get; }
	}
}