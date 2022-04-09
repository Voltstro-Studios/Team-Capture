// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Globalization;
using Team_Capture.Core.Networking.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.Elements
{
    /// <summary>
    ///     A button for joining a server
    /// </summary>
    internal class JoinServerButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gameNameText;
        [SerializeField] private TextMeshProUGUI mapNameText;
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private TextMeshProUGUI playerCountText;

        [SerializeField] private Button baseButton;

        /// <summary>
        ///     Sets up this button for connecting to a server
        /// </summary>
        /// <param name="server">The server details</param>
        /// <param name="call">The connect action</param>
        internal void SetupConnectButton(TCServerResponse server, Action call)
        {
            baseButton.onClick.AddListener(delegate { call(); });

            SetTextValues(server);
        }

        /// <summary>
        ///     Sets the text on the button
        /// </summary>
        /// <param name="server"></param>
        internal void SetTextValues(TCServerResponse server)
        {
            gameNameText.text = server.GameName.String;
            mapNameText.text = server.SceneName;
            playerCountText.text = $"{server.CurrentAmountOfPlayers}/{server.MaxPlayers}";
            
            //Calculate ping
            double rounded = Math.Round(server.TimeDifference * 1000, 0);
            
            pingText.text = rounded.ToString(CultureInfo.InvariantCulture);
        }
    }
}