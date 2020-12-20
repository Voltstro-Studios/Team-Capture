namespace Team_Capture.Settings.Enums
{
	/// <summary>
	///     VSync options
	/// </summary>
	internal enum VSync
	{
		/// <summary>
		///     No VSync, this is best in high action games
		/// </summary>
		Disable = 0,

		/// <summary>
		///     Sync the framerate to the framerate of your monitor
		/// </summary>
		EveryVBlank = 1,

		/// <summary>
		///     Half the framerate of your monitor
		/// </summary>
		EverySecondVBlank = 2
	}
}