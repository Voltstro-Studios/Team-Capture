using Delegates;

namespace Core.Console
{
	public class ConsoleCommand
	{
		/// <summary>
		/// The summary of the command
		/// </summary>
		public string CommandSummary { get; set; }

		/// <summary>
		/// Min amount of args required (Optional)
		/// </summary>
		public int MinArgs { get; set; }

		/// <summary>
		/// Max amount of args required (Optional)
		/// </summary>
		public int MaxArgs { get; set; }

		/// <summary>
		/// The method for the command
		/// </summary>
		public MethodDelegate CommandMethod { get; set; }
	}
}
