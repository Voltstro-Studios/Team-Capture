using System;

namespace Helper
{
	/// <summary>
	/// Helper for time
	/// </summary>
	public static class TimeHelper
	{
		/// <summary>
		/// Gets Unix time now in total seconds
		/// </summary>
		/// <returns></returns>
		public static long UnixTimeNow()
		{
			TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
			return (long)timeSpan.TotalSeconds;
		}
	}
}