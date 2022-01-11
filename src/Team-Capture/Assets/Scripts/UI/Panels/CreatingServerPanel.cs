// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.AddressablesAddons;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Team_Capture.UI.Panels
{
    /// <summary>
    ///     A panel that is displayed when the server is starting
    /// </summary>
    internal class CreatingServerPanel : PanelBase
    {
        /// <summary>
        ///     Whats the locale to use for the starting message
        /// </summary>
        [Tooltip("Whats the locale to use for the starting message")]
        public CachedLocalizedString startingMessageLocale;

        /// <summary>
        ///     Whats the locale to use for the failed to start message
        /// </summary>
        [Tooltip("Whats the locale to use for the failed to start message")]
        public CachedLocalizedString failedToStartMessageLocale;

        /// <summary>
        ///     <see cref="LocalizedString" /> for the failed to connect message
        /// </summary>
        [Tooltip("LocalizedString for the failed to connect message")]
        public CachedLocalizedString failedToConnectMessageLocale;

        /// <summary>
        ///     The text object of where the text will be placed
        /// </summary>
        public TMP_Text messageText;

        private void Start()
        {
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public override void OnEnable()
        {
            base.OnEnable();
            cancelButton.interactable = false;
            messageText.text = startingMessageLocale.Value;
        }

        /// <summary>
        ///     Call this when the server process fails to start
        /// </summary>
        public void FailedToStartMessage()
        {
            cancelButton.interactable = true;
            messageText.text = failedToStartMessageLocale.Value;
        }

        /// <summary>
        ///     Call this when the client fails to connect to the started server
        /// </summary>
        public void FailedToConnectMessage()
        {
            cancelButton.interactable = true;
            messageText.text = failedToConnectMessageLocale.Value;
        }
    }
}