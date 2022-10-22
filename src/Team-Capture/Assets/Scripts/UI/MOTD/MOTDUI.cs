// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Core.Networking;
using Team_Capture.Logging;
using Team_Capture.UserManagement;
using TMPro;
using UnityEngine;
using VoltstroStudios.UnityWebBrowser;

namespace Team_Capture.UI.MOTD
{
    /// <summary>
    ///     UI for displaying MOTDs
    /// </summary>
    internal class MOTDUI : MonoBehaviour
    {
        private const string InjectJavaScriptCode = @"
function hideControls() {
    const navControls = document.getElementById('controls-nav');
        if(typeof(navControls) !== 'undefined') {
            navControls.style.visibility = 'hidden';
            document.getElementById('controls-footer-social').style.visibility = 'hidden';
        }
    }

    function hideAllControls() {
        const navControls = document.getElementById('controls-nav');
        if(typeof(navControls) !== 'undefined') {
            navControls.style.visibility = 'hidden';
            document.getElementById('footer').style.visibility = 'hidden';
        }
    }

    function setName(name) {
        const playerName = document.getElementById('playerName');
        if(typeof(playerName) !== 'undefined') {
            playerName.innerText = name;
        }
    }
";

        [SerializeField] private TextMeshProUGUI motdTitleText;
        [SerializeField] private TextMeshProUGUI motdText;

        [SerializeField] private GameObject motdTextScroll;
        [SerializeField] private WebBrowserUIBasic webBrowserUI;

        private Action onCloseAction;

        /// <summary>
        ///     Setup the MOTD UI
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <param name="onClose"></param>
        internal void Setup(ServerConfig serverConfig, Action onClose)
        {
            motdTitleText.text = $"{serverConfig.GameName.String}'s MOTD.";

            if (serverConfig.MotdMode == Server.ServerMOTDMode.WebOnly ||
                serverConfig.MotdMode == Server.ServerMOTDMode.WebWithTextBackup &&
                Client.ClientMotdMode == Client.ClientMOTDMode.WebSupport)
            {
                webBrowserUI.browserClient.Logger = new TCWebBrowserLogger();
                webBrowserUI.browserClient.initialUrl = serverConfig.MotdUrl.String;
                webBrowserUI.browserClient.OnLoadFinish += OnLoadFinish;

                motdTextScroll.SetActive(false);
                webBrowserUI.gameObject.SetActive(true);
            }

            else if (serverConfig.MotdMode == Server.ServerMOTDMode.TextOnly ||
                     serverConfig.MotdMode == Server.ServerMOTDMode.WebWithTextBackup)
            {
                if (string.IsNullOrWhiteSpace(serverConfig.MotdText.String))
                    throw new InvalidMOTDSettings(
                        "The server's MOTD was set to text, however the sent over text contained nothing!");

                motdTextScroll.SetActive(true);
                webBrowserUI.gameObject.SetActive(false);

                motdText.text = serverConfig.MotdText.String;
            }

            onCloseAction = onClose;
        }

        private void OnLoadFinish(string url)
        {
            string javaScriptCode = $"{InjectJavaScriptCode}\n" +
                                    "hideControls();\n" +
                                    $"setName(\"{User.GetActiveUser().UserName}\");";

            webBrowserUI.ExecuteJs(javaScriptCode);
        }

        public void CloseMOTD()
        {
            onCloseAction.Invoke();
            Destroy(gameObject);
        }
    }
}