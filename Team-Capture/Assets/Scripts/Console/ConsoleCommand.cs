// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Console
{
	/// <summary>
	///     A command that is used in the console
	/// </summary>
	public class ConsoleCommand
	{
		/// <summary>
		///     The summary of the command
		/// </summary>
		public string CommandSummary { get; set; }

		/// <summary>
		///     Min amount of args required (Optional)
		/// </summary>
		public int MinArgs { get; set; }

		/// <summary>
		///     Max amount of args required (Optional)
		/// </summary>
		public int MaxArgs { get; set; }

		/// <summary>
		///     Sets what this command can and cannot run on
		/// </summary>
		public CommandRunPermission RunPermission { get; set; }

		/// <summary>
		///     The method for the command
		/// </summary>
		internal ConsoleBackend.MethodDelegate CommandMethod { get; set; }
	}
}