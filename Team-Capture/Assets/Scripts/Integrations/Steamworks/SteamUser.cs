// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Threading.Tasks;
using Mirror;
using Steamworks;
using Steamworks.Data;
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

        private Texture2D userProfilePicture;

        public Texture UserProfilePicture
        {
            get
            {
                if (userProfilePicture != null) 
                    return userProfilePicture;
                
                if (SteamClient.IsLoggedOn)
                {
                    //TODO: Steam client avatar cache stuff
                    //From my understanding of the Steamworks docs, if our client doesn't have a cache of the user avatar, it will go out and fetch it
                    //and trigger an AvatarImageLoaded event in SteamFriends, of which then we need to call this again.
                    Task<Image?> imageTask = SteamFriends.GetLargeAvatarAsync(UserId);
                    imageTask.Wait();
                    if (!imageTask.Result.HasValue)
                        return null;

                    Image image = imageTask.Result.Value;
                    userProfilePicture = new Texture2D((int)image.Height, (int)image.Width, TextureFormat.RGBA32, false, false);
                    userProfilePicture.LoadSteamworksImageIntoTexture2D(image);
                }
                else
                {
                    //If we do not have client abilities, just create a blank 512 texture
                    userProfilePicture = new Texture2D(512, 512);
                }

                return userProfilePicture;
            }
        }

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