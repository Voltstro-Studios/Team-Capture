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
    internal class Account
    {
	    public AccountProvider AccountProvider;

	    public string AccountName;

	    public ulong AccountId;

	    public Uri AccountProfileURL;

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