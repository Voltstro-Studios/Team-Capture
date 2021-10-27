// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Steamworks;
using Steamworks.Data;
using Team_Capture.UserManagement;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Integrations.Steamworks
{
    /// <summary>
    ///     An <see cref="IUser" /> that uses Steamworks
    /// </summary>
    public class SteamUser : IUser
    {
        /// <summary>
        ///     Steam <see cref="AuthTicket" /> of the user
        /// </summary>
        public AuthTicket AuthTicket;

        private string userName;

        private Texture2D userProfilePicture;

        /// <summary>
        ///     Creates a new <see cref="SteamUser" /> from the user's <see cref="SteamId" />
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
        ///     The <see cref="SteamId" /> of the user
        /// </summary>
        public ulong UserId { get; }

        public Texture UserProfilePicture
        {
            get
            {
                if (userProfilePicture != null)
                    return userProfilePicture;

                //The Steamworks API docs says that GetLargeFriendAvatar should be returning as 128*128 image.
                //However I am getting 184*184 image... We will just resize the texture if it doesn't match
                userProfilePicture = new Texture2D(184, 184, TextureFormat.RGBA32, false, false);

                if (SteamClient.IsLoggedOn)
                    LoadSteamAvatarAsync().Forget();

                return userProfilePicture;
            }
        }

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
            writer.WriteByte((byte) UserProvider);
            writer.WriteULong(UserId);
            writer.WriteArray(AuthTicket.Data);
        }

        private async UniTaskVoid LoadSteamAvatarAsync()
        {
            var imageTask = await SteamFriends.GetLargeAvatarAsync(UserId).AsUniTask();
            if (!imageTask.HasValue)
                return;

            Image image = imageTask.Value;
            Logger.Debug("Got Steam user profile image of {Height} x {Width}", image.Height, image.Width);

            if (userProfilePicture.width != image.Width || userProfilePicture.height != image.Height)
                userProfilePicture.Reinitialize((int) image.Width, (int) image.Height);

            userProfilePicture.LoadSteamworksImageIntoTexture2D(image);
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