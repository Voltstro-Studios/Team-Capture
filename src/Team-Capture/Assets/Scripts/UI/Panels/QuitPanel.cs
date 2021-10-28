// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Core;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Team_Capture.UI.Panels
{
    internal class QuitPanel : PanelBase
    {
        public AssetReference quitMessagesAsset;

        /// <summary>
        ///     The text for where the quit messages will go
        /// </summary>
        [Tooltip("The text for where the quit messages will go")]
        public TextMeshProUGUI quitSentenceText;

        private QuitMessages quitMessages;

        private void Awake()
        {
            quitMessages = quitMessagesAsset.LoadAssetAsync<QuitMessages>().WaitForCompletion();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (quitMessages != null)
            {
                quitSentenceText.text = quitMessages.quitMessages[Random.Range(0, quitMessages.quitMessages.Length)];
                return;
            }

            quitSentenceText.text = "When you are missing quit messages...";
        }

        /// <summary>
        ///     Quits from the game
        /// </summary>
        public void Quit()
        {
            if (NetworkManager.singleton.isNetworkActive)
                NetworkManager.singleton.StopHost();

            Game.QuitGame();
        }
    }
}