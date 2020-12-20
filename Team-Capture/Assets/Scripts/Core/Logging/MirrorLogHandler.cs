using System;
using kcp2k;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Team_Capture.Core.Logging
{
	internal class MirrorLogHandler : ILogHandler
	{
		internal MirrorLogHandler()
		{
			//Setup kcp2k's logger to point to Mirror's log handler, which in turn will log to our logger
			Log.Error = message => LogFormat(LogType.Error, null, message);
			Log.Warning = message => LogFormat(LogType.Warning, null, message);
			Log.Info = message => LogFormat(LogType.Log, null, message);
		}

		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			switch (logType)
			{
				case LogType.Exception:
				case LogType.Error:
					Logger.Error(string.Format(format, args));
					break;
				case LogType.Assert:
				case LogType.Warning:
					Logger.Warn(string.Format(format, args));
					break;
				case LogType.Log:
					Logger.Info(string.Format(format, args));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
			}
		}

		public void LogException(Exception exception, Object context)
		{
			Logger.Error("Exception: {@Ex}", exception);
		}
	}
}