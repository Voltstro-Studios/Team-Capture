// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Console
{
    /// <summary>
    ///     What run permission does this console command have
    /// </summary>
    public enum CommandRunPermission
    {
        /// <summary>
        ///     This command can be run in both client/offline mode, as well as in server mode
        /// </summary>
        Both,

        /// <summary>
        ///     This command can only be run in server mode
        /// </summary>
        ServerOnly,

        /// <summary>
        ///     This command can only be run in client/offline mode
        /// </summary>
        ClientOnly
    }
}