// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Core.Compression;
using UnityEngine;

namespace Team_Capture.Core.UserAccount
{
	/// <summary>
	///		Contains data about a user
	/// </summary>
    internal class Account
    {
	    /// <summary>
	    ///		Who provides this account
	    /// </summary>
	    public AccountProvider AccountProvider;

	    /// <summary>
	    ///		The account name (or username)
	    /// </summary>
	    public string AccountName;

	    /// <summary>
	    ///		The account ID
	    /// </summary>
	    public ulong AccountId;

	    /// <summary>
	    ///		Link to their profile picture
	    /// </summary>
	    public Uri AccountProfileURL;

	    /// <summary>
	    ///		<see cref="Texture2D"/> of their profile picture
	    /// </summary>
	    public Texture2D AccountProfile;

	    public NetworkedAccount ToNetworked()
	    {
		    return new NetworkedAccount
		    {
			    AccountProvider = AccountProvider,
			    AccountId = AccountId,
			    AccountName = new CompressedNetworkString(AccountName)
		    };
	    }
    }
}