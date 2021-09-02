// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Mirror;
using Steamworks;
using Team_Capture.UserManagement;
using UnityEngine;

namespace Team_Capture.Integrations.Steamworks
{
    /// <summary>
    ///     An <see cref="IUser"/> that uses Steamworks
    /// </summary>
    public class SteamUser : IUser
    {
        /// <summary>
        ///     Creates a new <see cref="SteamUser"/> from the user's <see cref="SteamId"/>
        /// </summary>
        /// <param name="id"></param>
        public SteamUser(SteamId id)
        {
            UserId = id;
        }

        private SteamUser(SteamId id, byte[] authData)
        {
            UserId = id;
            AuthTicket = new AuthTicket
            {
                Data = authData
            };
        }

        public UserProvider UserProvider => UserProvider.Steam;

        private string userName;
        public string UserName
        {
            get
            {
                if (userName != null)
                    return userName;

                if (!SteamClient.IsLoggedOn)
                {
                    string userIdAsString = UserId.ToString();
                    userName = userIdAsString;
                    return userIdAsString;
                }

                if (UserId == SteamClient.SteamId)
                {
                    string steamClientName = SteamClient.Name;
                    userName = steamClientName;
                    return steamClientName;
                }

                string friendName = SteamFriends.GetFriendPersonaName(UserId);
                userName = friendName;
                return friendName;
            }
        }

        /// <summary>
        ///     The <see cref="SteamId"/> of the user
        /// </summary>
        public ulong UserId { get; }

        public Texture UserProfilePicture { get; }

        /// <summary>
        ///     Steam <see cref="AuthTicket"/> of the user
        /// </summary>
        public AuthTicket AuthTicket;
        
        public void ServerStartClientAuthentication(Action onSuccess, Action onFail)
        {
            SteamServerManager.BeginAuthUser(this, onSuccess, onFail);
        }

        public void ServerCancelClientAuthentication()
        {
            SteamServerManager.CancelAuthUser(this);
        }

        public void ClientStartAuthentication()
        {
            AuthTicket = global::Steamworks.SteamUser.GetAuthSessionTicket();
        }

        public void ClientStopAuthentication()
        {
            AuthTicket.Cancel();
        }

        public void WriteNetwork(NetworkWriter writer)
        {
            writer.WriteByte((byte)UserProvider);
            writer.WriteULong(UserId);
            writer.WriteArray(AuthTicket.Data); 
        }

        internal static IUser Create(NetworkReader reader)
        {
            return new SteamUser(reader.ReadULong(), reader.ReadArray<byte>());
        }
        
        public override bool Equals(object obj)
        {
            if (obj is SteamUser steamUser)
                return steamUser.UserId == UserId;
            
            return false;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
    }
}