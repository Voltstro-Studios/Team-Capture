// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;

namespace Team_Capture.Helper
{
	/// <summary>
	///     Helper for time
	/// </summary>
	public static class TimeHelper
	{
		/// <summary>
		///     Gets Unix time now in total seconds
		/// </summary>
		/// <returns></returns>
		public static long UnixTimeNow()
		{
			TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
			return (long) timeSpan.TotalSeconds;
		}
	}
}