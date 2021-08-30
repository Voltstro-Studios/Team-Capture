// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Team_Capture.Integrations.Steamworks;
using UnityEngine.Scripting;

namespace Team_Capture.UserManagement
{
    public interface IUser
    {
        public UserProvider UserProvider { get; }
        
        public string UserName { get; }
        
        public ulong UserId { get; set; }

        public void ServerIsClientAuthenticated(Action onSuccess, Action onFail);
        
        public void ClientStartAuthentication();
        public void ClientStopAuthentication();

        public NetworkWriter WriteNetwork(NetworkWriter writer);
    }

    [Preserve]
    public static class UserReaderWriter
    {
        public static void Write(this NetworkWriter writer, IUser user)
        {
            user.WriteNetwork(writer);
        }

        public static IUser Read(this NetworkReader reader)
        {
            //We first need to figure out what account type we are dealing with
            UserProvider provider = (UserProvider)reader.ReadByte();
            
            if (provider == UserProvider.Offline)
            {
                return OfflineUser.Create(reader);
            }
            else
            {
                return SteamUser.Create(reader);
            }
        }
    }
}