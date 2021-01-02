using Mirror;
using Team_Capture.Core.Networking.Messages;
using UnityEngine;

namespace Team_Capture.Player
{
	/// <summary>
	///     Handles messages from the server
	/// </summary>
	internal sealed class PlayerServerMessages : MonoBehaviour
	{
		private PlayerUIManager uiManager;

		private void Awake()
		{
			//Register all our custom messages
			NetworkClient.RegisterHandler<PlayerDiedMessage>(PlayerDiedMessage);

			uiManager = GetComponent<PlayerUIManager>();
		}

		private void OnDestroy()
		{
			//Unregister our custom messages on destroy
			NetworkClient.UnregisterHandler<PlayerDiedMessage>();
		}

		/// <summary>
		///     Player died message, for killfeed
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="message"></param>
		private void PlayerDiedMessage(NetworkConnection conn, PlayerDiedMessage message)
		{
			uiManager.AddKillfeedItem(message);
		}
	}
}