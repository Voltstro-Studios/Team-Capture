using Mirror;
using Team_Capture.UI.Chat;
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
			NetworkClient.RegisterHandler<ChatMessage>(ChatMessage);

			uiManager = GetComponent<PlayerUIManager>();
		}

		private void OnDestroy()
		{
			//Unregister our custom messages on destroy
			NetworkClient.UnregisterHandler<PlayerDiedMessage>();
			NetworkClient.UnregisterHandler<ChatMessage>();
		}

		/// <summary>
		///     Player died message, for killfeed
		/// </summary>
		/// <param name="message"></param>
		private void PlayerDiedMessage(PlayerDiedMessage message)
		{
			uiManager.AddKillfeedItem(message);
		}
		
		/// <summary>
		///		Chat message
		/// </summary>
		/// <param name="message"></param>
		private void ChatMessage(ChatMessage message)
		{
			uiManager.AddChatMessage(message);
		}
	}
}