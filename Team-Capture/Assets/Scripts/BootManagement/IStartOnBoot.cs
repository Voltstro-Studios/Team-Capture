namespace Team_Capture.BootManagement
{
	/// <summary>
	///     An interface for starting something on boot
	/// </summary>
	internal interface IStartOnBoot
	{
		/// <summary>
		///     Called when this boot-able object is created
		/// </summary>
		void Init();
	}
}