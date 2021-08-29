// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using Team_Capture.UserManagement;

namespace Team_Capture.Integrations.Steamworks
{
    public class SteamUser : IUser
    {
        public SteamUser(SteamId id, string userName)
        {
            UserId = id;
            UserName = userName;
        }
        
        public SteamUser(SteamId id, byte[] authData)
        {
            UserId = id;
            AuthTicket = new AuthTicket
            {
                Data = authData
            };
        }
        
        public UserProvider UserProvider { get; set; }
        
        public string UserName { get; set; }
        
        public ulong UserId { get; set; }

        public AuthTicket AuthTicket;
        
        public void ServerIsClientAuthenticated(Action onSuccess, Action onFail)
        {
            SteamServerManager.BeginAuthUser(this, onSuccess, onFail);
        }

        public void ClientStartAuthentication()
        {
            AuthTicket = global::Steamworks.SteamUser.GetAuthSessionTicket();
        }

        public void ClientStopAuthentication()
        {
            AuthTicket.Cancel();
        }

        public NetworkWriter WriteNetwork(NetworkWriter writer)
        {
            writer.WriteByte((byte)UserProvider);
            writer.WriteULong(UserId);
            writer.WriteArray(AuthTicket.Data);
            return writer;
        }

        internal static IUser Create(NetworkReader reader)
        {
            return new SteamUser(reader.ReadULong(), reader.ReadArray<byte>())
            {
                UserProvider = UserProvider.Steam
            };
        }
    }
}