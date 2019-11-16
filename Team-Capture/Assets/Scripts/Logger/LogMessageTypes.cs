﻿using System.Diagnostics;
using UnityEngine;

namespace HelperClasses.Logger
{
	/// <summary>
	/// A class that contains a unity formatted log message
	/// </summary>
	internal sealed class UnityLogMessage
	{
		internal readonly string Message;
		internal readonly string StackTrace;
		internal readonly LogType LogType;

		internal UnityLogMessage (string message, string stackTrace, LogType logType)
		{
			Message = message;
			StackTrace = stackTrace;
			LogType = logType;
		}
	}
//
//	/// <summary>
//	/// A custom formatted log message
//	/// </summary>
//	internal sealed class CustomLogMessage
//	{
//		internal readonly string Message;
//		internal readonly StackTrace StackTrace;
//		internal readonly StackFrame [] StackFrame;
//
//		internal CustomLogMessage (object message)
//		{
//			Message = message;
//			//Use (true) so we get the file info as well
//			StackTrace = new StackTrace(true);
//			StackFrame = StackTrace.GetFrames();
//		}
//	}
}