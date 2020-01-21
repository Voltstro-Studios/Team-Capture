using Core.Logger;
using DiscordRPC.Logging;

namespace GameManagers.Discord
{
	/// <summary>
	/// This is the custom <see cref="ILogger"/> for the <see cref="DiscordRPC.DiscordRpcClient"/> using TC's
	/// <see cref="Logger"/>
	/// </summary>
	public class TCDiscordLogger : ILogger
	{
		public LogLevel Level { get; set; }

		public void Trace(string message, params object[] args)
		{
			if (Level != LogLevel.Trace) return;
			Logger.Log($"[IPC Trace] {(args.Length > 0 ? string.Format(message, args) : message)}", LogVerbosity.Debug);
		}

		public void Info(string message, params object[] args)
		{
			if (Level != LogLevel.Info) return;
			Logger.Log($"[IPC] {(args.Length > 0 ? string.Format(message, args) : message)}");
		}

		public void Warning(string message, params object[] args)
		{
			if (Level != LogLevel.Warning) return;
			Logger.Log($"[IPC] {(args.Length > 0 ? string.Format(message, args) : message)}", LogVerbosity.Warn);
		}

		public void Error(string message, params object[] args)
		{
			if (Level != LogLevel.Error) return;
			Logger.Log($"[IPC] {(args.Length > 0 ? string.Format(message, args) : message)}", LogVerbosity.Error);
		}
	}
}