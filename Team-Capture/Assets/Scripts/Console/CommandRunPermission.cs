namespace Console
{
	/// <summary>
	/// What run permission does this console command have
	/// </summary>
	public enum CommandRunPermission
	{
		/// <summary>
		/// This command can be run in both client/offline mode, as well as in server mode
		/// </summary>
		Both,

		/// <summary>
		/// This command can only be run in server mode
		/// </summary>
		ServerOnly,

		/// <summary>
		/// This command can only be run in client/offline mode
		/// </summary>
		ClientOnly
	}
}