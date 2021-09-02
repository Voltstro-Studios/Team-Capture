// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using Jdenticon;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core.Networking;
using UnityCommandLineParser;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;
using Object = UnityEngine.Object;

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

        private string userProfileLastName;
        private Texture2D userProfilePicture;
        public Texture UserProfilePicture
        {
            get
            {
                if (userProfilePicture != null && userProfileLastName == PlayerName) 
                    return userProfilePicture;
                
                //Delete the old version of the profile picture if it exists
                if (userProfilePicture != null)
                {
                    Logger.Debug("Regenerating user profile picture...");
                    Object.Destroy(userProfilePicture);
                }              
                    
                userProfileLastName = UserName;
                userProfilePicture = new Texture2D(512, 512);
                
                MemoryStream iconStream = new MemoryStream();
                Identicon.FromValue(UserName, 512).SaveAsPng(iconStream);
                    
                //Reset the icon stream
                iconStream.Flush();
                iconStream.Position = 0;

                //Read the icon stream buffer into an array
                const int size = 512 * 512 * 4;
                byte[] textureData = new byte[size];
                iconStream.Read(textureData, 0, size);
                iconStream.Dispose();

                //Load the texture data into the texture 2D
                userProfilePicture.LoadImage(textureData);
                userProfilePicture.Apply();
                return userProfilePicture;
            }
        }

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