using System;
using Core.Networking.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
	/// <summary>
	/// A button for joining a server
	/// </summary>
	internal class JoinServerButton : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI gameNameText;
		[SerializeField] private TextMeshProUGUI mapNameText;
		[SerializeField] private TextMeshProUGUI pingText;
		[SerializeField] private TextMeshProUGUI playerCountText;

		[SerializeField] private Button baseButton;

		/// <summary>
		/// Sets up this button for connecting to a server
		/// </summary>
		/// <param name="server">The server details</param>
		/// <param name="call">The connect action</param>
		internal void SetupConnectButton(TCServerResponse server, Action call)
		{
			baseButton.onClick.AddListener(delegate
			{
				call();
			});

			gameNameText.text = server.GameName;
			mapNameText.text = server.SceneName;
			pingText.text = "0";
			playerCountText.text = $"{server.CurrentAmountOfPlayers}/{server.MaxPlayers}";
		}
	}
}