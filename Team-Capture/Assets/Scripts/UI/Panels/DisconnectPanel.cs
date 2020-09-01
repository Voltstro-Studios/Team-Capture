using Mirror;

namespace UI.Panels
{
	/// <summary>
	/// The panel for the disconnect dialog
	/// </summary>
	public class DisconnectPanel : MainMenuPanelBase
	{
		/// <summary>
		/// Disconnects from the current game
		/// </summary>
		public void DisconnectGame()
		{
			NetworkManager.singleton.StopHost();
		}
	}
}