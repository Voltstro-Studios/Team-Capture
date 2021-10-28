// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;

namespace Team_Capture.UI.Panels
{
    /// <summary>
    ///     The panel for the disconnect dialog
    /// </summary>
    internal class DisconnectPanel : PanelBase
    {
        /// <summary>
        ///     Disconnects from the current game
        /// </summary>
        public void DisconnectGame()
        {
            NetworkManager.singleton.StopHost();
        }
    }
}