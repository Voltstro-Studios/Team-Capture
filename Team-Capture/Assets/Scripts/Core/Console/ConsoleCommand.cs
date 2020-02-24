namespace Core.Console
{
	public class ConsoleCommand
	{
		public string CommandName { get; set; }
		public string CommandSummary { get; set; }
		public MethodDelegate CommandMethod { get; set; }
	}
}
