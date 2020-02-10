using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Logger;
using Core.Networking.Discovery;
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

		private void Awake()
		{
			gameDiscovery = TCNetworkManager.Instance.gameDiscovery;
		}

		private void OnEnable()
		{
			gameDiscovery.onServerFound.AddListener(AddServer);
			gameDiscovery.StartDiscovery();
		}

		private void OnDisable()
		{
			gameDiscovery.onServerFound.RemoveListener(AddServer);
			gameDiscovery.StopDiscovery();
			RefreshServerList();
		}

		private void AddServerItem(TCServerResponse server)
		{
			GameObject newItem = Instantiate(serverItemPrefab, serverListTransform, false);
			newItem.GetComponent<JoinServerButton>().SetupConnectButton(server);
		}

		public void RefreshServerList()
		{
			servers.Clear();
			for (int i = 0; i < serverListTransform.childCount; i++)
			{
				Destroy(serverListTransform.GetChild(i).gameObject);
			}
		}

		public void AddServer(TCServerResponse server)
		{
			if(servers.Any(x => Equals(x.EndPoint, server.EndPoint)))
				return;

			servers.Add(server);
			AddServerItem(server);

			Core.Logger.Logger.Log($"Found server at {server.EndPoint.Address}", LogVerbosity.Debug);
		}
	}
}
