﻿// ReSharper disable InconsistentNaming

namespace Logger
{
	public enum LogVerbosity
	{
		/// <summary>
		///     Errors only; only critical errors will be logged. It is not recommended to use this setting, use <see cref="WARN"/> or <see cref="INFO"/> instead
		/// </summary>
		ERROR = 0,

		/// <summary>
		///     Errors and warnings are logged. 
		/// </summary>
		WARN = 1,

		/// <summary>
		///     Messages that provides extra information, such as platform information etc
		/// </summary>
		INFO = 2,

		/// <summary>
		///     Record almost everything, to ease debugging. Not recommended for normal use.
		/// </summary>
		DEBUG = 3,

		/// <summary>
		///     Most verbose, record every single message. Do not use except when debugging, as log files will be much larger and performance will be lower.
		/// </summary>
		VERBOSE = 4
	}
}