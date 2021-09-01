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
    /// <summary>
    ///     An <see cref="IUser"/> contains details and methods about a user
    /// </summary>
    public interface IUser
    {
        /// <summary>
        ///     Who provides this user
        /// </summary>
        public UserProvider UserProvider { get; }
        
        /// <summary>
        ///     The username of this user
        /// </summary>
        public string UserName { get; }
        
        /// <summary>
        ///     The ID of the user
        ///     <para>This my be unused depending on the <see cref="UserProvider"/></para>
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        ///     Starts the authenticating the user
        /// </summary>
        /// <param name="onSuccess">Invoked when authentication is successful</param>
        /// <param name="onFail">Invoked when authentication fails</param>
        public void ServerStartClientAuthentication(Action onSuccess, Action onFail);

        /// <summary>
        ///     Cancels the authentication of a user
        /// </summary>
        public void ServerCancelClientAuthentication();
        
        /// <summary>
        ///     Starts authentication on the client end
        /// </summary>
        public void ClientStartAuthentication();
        
        /// <summary>
        ///     Stops authentication on the client end
        /// </summary>
        public void ClientStopAuthentication();

        /// <summary>
        ///     Write what you need to the <see cref="NetworkWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public void WriteNetwork(NetworkWriter writer);
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