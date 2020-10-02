using System;

namespace Helper
{
	public static class TimeHelper
	{
		public static long UnixTimeNow()
		{
			TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
			return (long)timeSpan.TotalSeconds;
		}
	}
}