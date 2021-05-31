// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Team_Capture.Console;
using UnityEngine;
using UnityCommandLineParser;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.UserAccount
{
	/// <summary>
	///		Provides a system to user information such as their profile and username
	/// </summary>
    internal static class User
    {
	    [CommandLineArgument("name")] 
	    [ConVar("name", "Sets the name", nameof(UpdateOfflineAccount), true)]
	    public static string PlayerName = "NotSet";

	    private static AccountProvider[] accountProvidersPriority;

	    private static SortedList<AccountProvider, Account> accounts;

		/// <summary>
		///		Initializes the user system
		/// </summary>
	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	    internal static void Init()
		{
			accountProvidersPriority = new[] {AccountProvider.Steam, AccountProvider.Discord, AccountProvider.Offline};
		    accounts = new SortedList<AccountProvider, Account>(new AccountProviderComparer());

			AddAccount(new Account
			{
				AccountProvider = AccountProvider.Offline,
				AccountName = PlayerName
			});

			Logger.Debug("Initialized user account system.");
	    }

		/// <summary>
		///		Adds an <see cref="Account"/> to the user system
		/// </summary>
		/// <param name="account"></param>
	    internal static void AddAccount(Account account)
	    {
		    if (accounts.ContainsKey(account.AccountProvider))
			    throw new UserAccountAlreadyExistsException($"The user account provider {account.AccountProvider} already exists!");

		    accounts.Add(account.AccountProvider, account);
	    }

		/// <summary>
		///		Gets an <see cref="Account"/> that is provided by a certain <see cref="AccountProvider"/>
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
	    internal static Account GetAccount(AccountProvider provider)
	    {
		    return accounts.SingleOrDefault(x => x.Key == provider).Value;
	    }

		/// <summary>
		///		Gets the default <see cref="Account"/> based of loaded integrations and priorities
		/// </summary>
	    internal static Account DefaultAccount => accounts.First().Value;

		/// <summary>
		///		Gets all <see cref="Account"/>s as <see cref="NetworkedAccount"/>s
		/// </summary>
		/// <returns></returns>
		internal static NetworkedAccount[] GetAccountsAsNetworked()
		{
			List<NetworkedAccount> networkedAccounts = new List<NetworkedAccount>(accounts.Count);
			networkedAccounts.AddRange(User.accounts.Select(account => account.Value.ToNetworked()));
			return networkedAccounts.ToArray();
		}

		private static void UpdateOfflineAccount()
		{
			GetAccount(AccountProvider.Offline).AccountName = PlayerName;
		}

		private class AccountProviderComparer : IComparer<AccountProvider>
		{
			public int Compare(AccountProvider x, AccountProvider y)
			{
				int xIndex = Array.IndexOf(accountProvidersPriority, x);
				int yIndex = Array.IndexOf(accountProvidersPriority, y);

				return xIndex.CompareTo(yIndex);
			}
		}
    }
}