using System.Net;
using System.Threading.Tasks;
using Core.Networking.Discovery;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class JoinGamePanel : MainMenuPanelBase
	{
		private TCServerResponse[] servers;

		[SerializeField] private GameObject serverItemPrefab;

		[SerializeField] private Transform serverListTransform;

		private void Start()
		{
			servers = TCGameDiscovery.GetServers();

			AddAllServersToServerList();
		}

		private void AddAllServersToServerList()
		{
			foreach (TCServerResponse server in servers)
			{
				GameObject newItem = Instantiate(serverItemPrefab, serverListTransform, false);
				newItem.GetComponent<Button>().onClick.AddListener(delegate { ConnectToServer(server.EndPoint); });
			}
		}

		public void ConnectToServer(IPEndPoint ip)
		{
			//TODO: Better connection stuff with Lite Net Lib 4 Mirror Transport
			NetworkManager.singleton.networkAddress = ip.Address.ToString();
			NetworkManager.singleton.StartClient();
		}
	}
}
