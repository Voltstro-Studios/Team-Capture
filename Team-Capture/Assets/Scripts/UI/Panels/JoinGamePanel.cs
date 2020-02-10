using System.Collections.Generic;
using System.Linq;
using Core.Logger;
using Core.Networking.Discovery;
using UnityEngine;

namespace UI.Panels
{
	public class JoinGamePanel : MainMenuPanelBase
	{
		private readonly List<TCServerResponse> servers = new List<TCServerResponse>();

		[SerializeField] private GameObject serverItemPrefab;

		[SerializeField] private Transform serverListTransform;

		private void Start()
		{
			TCGameDiscovery.OnServerFound.AddListener(AddServer);
		}

		private void OnDisable()
		{
			RefreshServerList();
		}

		private void OnDestroy()
		{
			TCGameDiscovery.OnServerFound.RemoveListener(AddServer);
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
