﻿﻿namespace Global
{
	public enum LogNamingMode
	{
		/// <summary>
		/// Log file names will be the date followed by an incremental number e.g. 2019-06-21 3, 2019-06-21 4
		/// </summary>
		Incremental,
		/// <summary>
		/// Log file names will be the date followed by the time e.g. 2019-06-21 16-30
		/// </summary>
		DateTime
	}
}