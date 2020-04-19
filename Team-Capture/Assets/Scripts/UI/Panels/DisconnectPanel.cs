using Mirror;

namespace UI.Panels
{
	public class DisconnectPanel : MainMenuPanelBase
	{
		public void DisconnectGame()
		{
			NetworkManager.singleton.StopHost();
		}
	}
}