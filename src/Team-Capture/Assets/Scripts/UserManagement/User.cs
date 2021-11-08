// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using NetFabric.Hyperlinq;
using Team_Capture.Console;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UserManagement
{
    /// <summary>
    ///     Provides user management services
    /// </summary>
    public static class User
    {
        private static SortedList<UserProvider, IUser> users;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Init()
        {
            users = new SortedList<UserProvider, IUser>(new UserProviderComparer());
            AddUser(new OfflineUser());
        }

        [ConCommand("whoami", "Gets who you are")]
        private static void WhoAmICommand(string[] args)
        {
            IUser user = GetActiveUser();
            Logger.Info("{UserName} ({ID}) provided by {Provider}", user.UserName, user.UserId, user.UserProvider);
        }

        /// <summary>
        ///     Adds a user
        /// </summary>
        /// <param name="user"></param>
        public static void AddUser(IUser user)
        {
            Logger.Debug("Added user of type {Provider}", user.UserProvider);
            users.Add(user.UserProvider, user);
        }

        /// <summary>
        ///     Gets the active (main) <see cref="IUser" />
        /// </summary>
        /// <returns></returns>
        public static IUser GetActiveUser()
        {
            Option<KeyValuePair<UserProvider, IUser>> result = users.AsValueEnumerable().First();
            return result.IsNone ? null : result.Value.Value;
        }

        /// <summary>
        ///     Gets all <see cref="IUser" />s
        /// </summary>
        /// <returns></returns>
        internal static IUser[] GetUsers()
        {
            return users.AsValueEnumerable()
                .Select(x => x.Value)
                .ToArray();
        }

        private class UserProviderComparer : IComparer<UserProvider>
        {
            public int Compare(UserProvider x, UserProvider y)
            {
                return x.CompareTo(y);
            }
        }
    }
}