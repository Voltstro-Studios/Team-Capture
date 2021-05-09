using Mirror;
using TMPro;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.Chat
{
    internal class Chat : MonoBehaviour
    {
        public TMP_Text text;
        public TMP_InputField inputField;
        public GameObject hideableSection;

        public bool IsChatOpen => hideableSection.activeSelf;

        internal void Submit()
        {
            if (inputField.isFocused)
            {
                SendChatMessage(new ChatMessage(inputField.text));
                inputField.text = "";
            }
        }

        internal void AddMessage(ChatMessage message)
        {
            text.text += $"{message.Player}: {message.Message}\n";
            Logger.Info($"Chat: {message.Player}: {message.Message}");
        }

        internal void SendChatMessage(ChatMessage message)
        {
            NetworkClient.connection.Send(message, Channels.Unreliable);
        }

        internal void ToggleChat()
        {
            hideableSection.SetActive(!IsChatOpen);

            if (!IsChatOpen) return;
            
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
