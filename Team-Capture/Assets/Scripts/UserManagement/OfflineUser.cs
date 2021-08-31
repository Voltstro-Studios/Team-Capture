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
    public class OfflineUser : IUser
    {
        [CommandLineArgument("name")] 
        [ConVar("name", "Sets the name", true)]
        public static string PlayerName = "NotSet";

        public OfflineUser(string userName = null)
        {
            serverName = userName;
        }
        
        public UserProvider UserProvider => UserProvider.Offline;

        private readonly string serverName;
        public string UserName => TCNetworkManager.IsServer ? serverName : PlayerName;

        public ulong UserId { get; set; } = 0;

        public void ServerIsClientAuthenticated(Action onSuccess, Action onFail)
        {
            onSuccess();
        }

        public void ClientStartAuthentication()
        {
        }

        public void ClientStopAuthentication()
        {
        }

        public NetworkWriter WriteNetwork(NetworkWriter writer)
        {
            writer.WriteByte((byte)UserProvider);
            writer.WriteString(UserName);
            return writer;
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