using Mirror;

namespace Team_Capture.UI.Panels
{
	/// <summary>
	///     The panel for the disconnect dialog
	/// </summary>
	internal class DisconnectPanel : MainMenuPanelBase
	{
		/// <summary>
		///     Disconnects from the current game
		/// </summary>
		public void DisconnectGame()
		{
			NetworkManager.singleton.StopHost();
		}
	}
}