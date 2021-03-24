using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core.UserAccount
{
    internal static class User
    {
	    private static List<AccountProvider> accountProvidersPriority;

	    private static SortedList<AccountProvider, Account> accounts;

	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	    internal static void Init()
	    {
		    accountProvidersPriority = new List<AccountProvider>
		    {
			    AccountProvider.Steam,
			    AccountProvider.Discord,
			    AccountProvider.Offline
		    };
		    accounts = new SortedList<AccountProvider, Account>(new AccountProviderComparer());
			Logger.Debug("Initialized user account system.");
	    }

	    internal static void AddAccount(Account account)
	    {
		    if (accounts.ContainsKey(account.AccountProvider))
			    throw new UserAccountAlreadyExistsException($"The user account provider {account.AccountProvider} already exists!");

		    accounts.Add(account.AccountProvider, account);
	    }

	    internal static Account GetAccount(AccountProvider provider)
	    {
		    return accounts.SingleOrDefault(x => x.Key == provider).Value;
	    }

	    internal static Account DefaultAccount => accounts.First().Value;

		private class AccountProviderComparer : IComparer<AccountProvider>
		{
			public int Compare(AccountProvider x, AccountProvider y)
			{
				int xIndex = accountProvidersPriority.IndexOf(x);
				int yIndex = accountProvidersPriority.IndexOf(y);

				return xIndex.CompareTo(yIndex);
			}
		}
    }
}