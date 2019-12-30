using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class DisconnectPanel : MonoBehaviour
	{
		public Button noBtn;

		public void DisconnectGame()
		{
			NetworkManager.singleton.StopHost();
		}
	}
}
