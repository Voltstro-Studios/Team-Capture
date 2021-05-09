using Mirror;
using Team_Capture.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.Chat
{
    internal class Chat : MonoBehaviour
    {
        public TMP_InputField inputField;

        public float previewTextDestroyDelayTime = 8.0f;
        
        public GameObject chatTextPrefab;

        public GameObject preview;
        public GameObject mainView;

        public ScrollRect previewScroll;
        public ScrollRect mainViewScroll;
        
        public Transform previewTextViewport;
        public Transform mainViewTextViewport;

        public bool IsChatOpen => mainView.activeSelf;

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
            string formattedMessage = $"{message.Player}: {message.Message.String}";
            TMP_Text mainViewText = Instantiate(chatTextPrefab, mainViewTextViewport, false).GetComponent<TMP_Text>();
            mainViewText.text = formattedMessage;

            TMP_Text previewText = Instantiate(chatTextPrefab, previewTextViewport, false).GetComponent<TMP_Text>();
            previewText.text = formattedMessage;
            previewText.gameObject.AddComponent<TimedDestroyer>().destroyDelayTime = previewTextDestroyDelayTime;
            
            Canvas.ForceUpdateCanvases();
            mainViewScroll.normalizedPosition = new Vector2(0, 0);
            previewScroll.normalizedPosition = new Vector2(0, 0);

            Logger.Info($"Chat: {formattedMessage}");
        }

        internal void SendChatMessage(ChatMessage message)
        {
            NetworkClient.connection.Send(message, Channels.Unreliable);
        }

        internal void ToggleChat()
        {
            if(inputField.isFocused && IsChatOpen)
                return;
            
            mainView.SetActive(!IsChatOpen);
            preview.SetActive(!IsChatOpen);
            
            mainViewScroll.normalizedPosition = new Vector2(0, 0);
            previewScroll.normalizedPosition = new Vector2(0, 0);

            if (!IsChatOpen) return;
            
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
