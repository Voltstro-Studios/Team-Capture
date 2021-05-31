// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.Chat
{
    /// <summary>
    ///     Text based chat UI controller
    /// </summary>
    internal class Chat : MonoBehaviour
    {
        /// <summary>
        ///     Input where the player inputs what they want to say
        /// </summary>
        public TMP_InputField inputField;

        /// <summary>
        ///     How long until the preview text stays for
        /// </summary>
        public float previewTextDestroyDelayTime = 8.0f;
        
        /// <summary>
        ///     The prefab for the text
        /// </summary>
        public GameObject chatTextPrefab;

        /// <summary>
        ///     Preview <see cref="GameObject"/>
        /// </summary>
        public GameObject preview;
        
        /// <summary>
        ///     Main view <see cref="GameObject"/>
        /// </summary>
        public GameObject mainView;

        /// <summary>
        ///     Preview <see cref="ScrollRect"/>
        /// </summary>
        public ScrollRect previewScroll;
        
        /// <summary>
        ///     Main view <see cref="ScrollRect"/>
        /// </summary>
        public ScrollRect mainViewScroll;
        
        /// <summary>
        ///     Preview content <see cref="Transform"/>
        /// </summary>
        public Transform previewTextViewport;
        
        /// <summary>
        ///     Main view content <see cref="Transform"/>
        /// </summary>
        public Transform mainViewTextViewport;

        /// <summary>
        ///     Is the chat opened?
        /// </summary>
        public bool IsChatOpen => mainView.activeSelf;

        /// <summary>
        ///     Submits the chat message
        /// </summary>
        internal void Submit()
        {
            if (inputField.isFocused)
                SendChatMessage();
        }

        /// <summary>
        ///     Sends the chat message to the server
        /// </summary>
        public void SendChatMessage()
        {
            SendChatMessage(new ChatMessage(inputField.text));
            inputField.text = "";
        }

        /// <summary>
        ///     Adds a message to the text
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        ///     Sends a <see cref="ChatMessage"/> to the server
        /// </summary>
        /// <param name="message"></param>
        internal void SendChatMessage(ChatMessage message)
        {
            NetworkClient.connection.Send(message, Channels.Unreliable);
        }

        /// <summary>
        ///     Toggles the chat
        /// </summary>
        internal void ToggleChat()
        {
            if(inputField.isFocused && IsChatOpen)
                return;
            
            ActivateChat(!IsChatOpen);
        }

        /// <summary>
        ///     Activates the chat
        /// </summary>
        /// <param name="state"></param>
        internal void ActivateChat(bool state)
        {
            mainView.SetActive(state);
            preview.SetActive(!state);
            
            mainViewScroll.normalizedPosition = new Vector2(0, 0);
            previewScroll.normalizedPosition = new Vector2(0, 0);

            if (!state) return;
            
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
