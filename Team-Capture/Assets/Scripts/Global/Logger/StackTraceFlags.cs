﻿﻿#region

  using System;

  #endregion

namespace Global
{
	[Flags]
	public enum StackTraceFlags
	{
		/// <summary>
		///     No flags, minimal information
		/// </summary>
		None = 0,

		/// <summary>
		///     Include parameter types e.g. Int32, String
		/// </summary>
		ParameterTypes = 1,

		/// <summary>
		///     Include names of parameters e.g. message, verbosity, exception
		/// </summary>
		ParameterNames = 1 << 1,

		/// <summary>
		///     Add a namespace to each type e.g. CustomLogger.Logger.Log
		/// </summary>
		Namespace = 1 << 2,

		/// <summary>
		///     Include information about the files e.g. E:/TestFile.cs line 14
		/// </summary>
		FileInfo = 1 << 3,

		/// <summary>
		///     Include unity functions in the stack trace as well e.g. UnityEngine.EventSystems.EventSystem.Update()
		/// </summary>
		IncludeUnity = 1 << 4
	}
}