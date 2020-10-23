namespace BootManagement
{
	/// <summary>
	/// An interface for starting something on boot
	/// </summary>
	public interface IStartOnBoot
	{
		/// <summary>
		/// Called when this boot-able object is created
		/// </summary>
		void Init();
	}
}