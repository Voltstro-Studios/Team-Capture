using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirror;
using Team_Capture.Core.Networking;
using Team_Capture.Core.Networking.Discovery;
using TMPro;
using UI.Elements;
using UnityEngine;
using Logger = Team_Capture.Core.Logging.Logger;

namespace UI.Panels
{
	/// <summary>
	///     A panel for joining games
	/// </summary>
	internal class JoinGamePanel : MainMenuPanelBase
	{
		/// <summary>
		///     The prefab for a server button
		/// </summary>
		[Tooltip("The prefab for a server button")]
		public GameObject serverItemPrefab;

		/// <summary>
		///     The <see cref="Transform" /> for the server list
		/// </summary>
		public Transform serverListTransform;

		/// <summary>
		///     Text for the current status
		/// </summary>
		public TextMeshProUGUI statusText;

		private readonly List<TCServerResponse> servers = new List<TCServerResponse>();

		private TCGameDiscovery gameDiscovery;

		private void Awake()
		{
			gameDiscovery = TCNetworkManager.Instance.gameDiscovery;
		}

		public override void OnEnable()
		{
			base.OnEnable();

			if (gameDiscovery == null) return;

			//Add a listener to the game discovery for when a server is found
			gameDiscovery.OnServerFound.AddListener(AddServer);
			gameDiscovery.StartDiscovery();
		}

		private void OnDisable()
		{
			if (gameDiscovery == null) return;

			//Remove the game discovery for when a server is found
			gameDiscovery.OnServerFound.RemoveListener(AddServer);
			gameDiscovery.StopDiscovery();
			RefreshServerList();
		}

		/// <summary>
		///     Refreshes the server by removing all servers
		/// </summary>
		public void RefreshServerList()
		{
			//Remove all servers
			servers.Clear();
			for (int i = 0; i < serverListTransform.childCount; i++)
				Destroy(serverListTransform.GetChild(i).gameObject);

			statusText.text = "Searching for games...";
			statusText.gameObject.SetActive(true);
		}

		/// <summary>
		///     Adds a <see cref="TCServerResponse" /> to the list of servers
		/// </summary>
		/// <param name="server"></param>
		public void AddServer(TCServerResponse server)
		{
			//We are connecting to a server...
			if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly) return;

			//If the server already exists in the list then ignore it
			if (servers.Any(x => Equals(x.EndPoint, server.EndPoint)))
				return;

			//Add the server to the list
			servers.Add(server);
			AddServerItem(server);

			statusText.gameObject.SetActive(false);

			Logger.Debug("Found server at {@Address}", server.EndPoint.Address);
		}

		/// <summary>
		///     Connects to a server
		/// </summary>
		/// <param name="ip"></param>
		public void ConnectToServer(IPEndPoint ip)
		{
			//Tell Mirror to connect to the server's IP
			NetworkManager.singleton.networkAddress = ip.Address.ToString();
			NetworkManager.singleton.StartClient();

			//Set our status text
			statusText.gameObject.SetActive(true);
			statusText.text = $"Connecting to '{ip.Address}'...";

			RefreshServerList();
		}

		private void AddServerItem(TCServerResponse server)
		{
			//Instantiate a new button for a server
			GameObject newItem = Instantiate(serverItemPrefab, serverListTransform, false);
			newItem.GetComponent<JoinServerButton>().SetupConnectButton(server, () => ConnectToServer(server.EndPoint));
		}
	}
}