// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;

namespace Team_Capture.Player
{
    /// <summary>
    ///     A player has died message
    /// </summary>
    internal struct PlayerDiedMessage : NetworkMessage
    {
        /// <summary>
        ///     Who was the victim
        /// </summary>
        public string PlayerKilled;

        /// <summary>
        ///     Who was the murderer
        /// </summary>
        public string PlayerKiller;

        /// <summary>
        ///     What weapon did they use
        /// </summary>
        public string WeaponName;
    }
}