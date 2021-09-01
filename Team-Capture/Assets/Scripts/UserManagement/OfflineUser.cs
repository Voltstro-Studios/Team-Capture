// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Networking;
using UnityCommandLineParser;

namespace Team_Capture.UserManagement
{
    /// <summary>
    ///     An offline <see cref="IUser"/>
    /// </summary>
    public class OfflineUser : IUser
    {
        /// <summary>
        ///     Default username for offline accounts
        /// </summary>
        [CommandLineArgument("name")] 
        [ConVar("name", "Sets the name", true)]
        public static string PlayerName = "NotSet";
        
        [ConVar("sv_offline_trim_name", "Will trim whitespace at the start and end of account names. " +
                                        "The local client will still see the untrimmed version.")]
        public static bool TrimUserNames = true;

        public OfflineUser(string userName = null)
        {
            if (userName == null) 
                return;

            serverName = TrimUserNames ? userName.TrimStart().TrimEnd() : userName;
        }
        
        public UserProvider UserProvider => UserProvider.Offline;

        private readonly string serverName;
        public string UserName => TCNetworkManager.IsServer ? serverName : PlayerName;

        /// <summary>
        ///     <see cref="UserId"/> is unused for <see cref="OfflineUser"/>
        /// </summary>
        public ulong UserId => 0;

        public void ServerStartClientAuthentication(Action onSuccess, Action onFail)
        {
            onSuccess();
        }

        public void ServerCancelClientAuthentication()
        {
        }

        public void ClientStartAuthentication()
        {
        }

        public void ClientStopAuthentication()
        {
        }

        public void WriteNetwork(NetworkWriter writer)
        {
            writer.WriteByte((byte)UserProvider);
            writer.WriteString(UserName);
        }

        internal static IUser Create(NetworkReader reader)
        {
            return new OfflineUser(reader.ReadString());
        }
        
        public override bool Equals(object obj)
        {
            if (obj is OfflineUser offlineUser)
            {
                return offlineUser.UserName == UserName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return UserName != null ? UserName.GetHashCode() : 0;
        }
    }
}