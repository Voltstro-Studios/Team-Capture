using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Logger;
using Core.Networking;
using Core.Networking.Discovery;
using Mirror;
using TMPro;
using UI.Elements;
using UnityEngine;

namespace UI.Panels
{
	public class JoinGamePanel : MainMenuPanelBase
	{
		private readonly List<TCServerResponse> servers = new List<TCServerResponse>();

		private TCGameDiscovery gameDiscovery;

		[SerializeField] private GameObject serverItemPrefab;

		[SerializeField] private Transform serverListTransform;

		[SerializeField] private TextMeshProUGUI statusText;

		private void Awake()
		{
			gameDiscovery = TCNetworkManager.Instance.gameDiscovery;
		}

		private void OnEnable()
		{
			if (gameDiscovery == null) return;

			gameDiscovery.onServerFound.AddListener(AddServer);
			gameDiscovery.StartDiscovery();
		}

		private void OnDisable()
		{
			if(gameDiscovery == null) return;

			gameDiscovery.onServerFound.RemoveListener(AddServer);
			gameDiscovery.StopDiscovery();
			RefreshServerList();
		}

		private void AddServerItem(TCServerResponse server)
		{
			GameObject newItem = Instantiate(serverItemPrefab, serverListTransform, false);
			newItem.GetComponent<JoinServerButton>().SetupConnectButton(server, () => ConnectToServer(server.EndPoint));
		}

		public void RefreshServerList()
		{
			servers.Clear();
			for (int i = 0; i < serverListTransform.childCount; i++)
			{
				Destroy(serverListTransform.GetChild(i).gameObject);
			}

			statusText.text = "Searching for games...";
			statusText.gameObject.SetActive(true);
		}

		public void AddServer(TCServerResponse server)
		{
			//We are connecting to a server...
			if(NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly) return;

			if(servers.Any(x => Equals(x.EndPoint, server.EndPoint)))
				return;

			servers.Add(server);
			AddServerItem(server);

			statusText.gameObject.SetActive(false);

			Core.Logger.Logger.Log($"Found server at {server.EndPoint.Address}", LogVerbosity.Debug);
		}

		public void ConnectToServer(IPEndPoint ip)
		{
			NetworkManager.singleton.networkAddress = ip.Address.ToString();
			NetworkManager.singleton.StartClient();

			statusText.gameObject.SetActive(true);
			statusText.text = $"Connecting to '{ip.Address}'...";

			RefreshServerList();
		}
	}
}