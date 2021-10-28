// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.UserManagement
{
    //TODO: We should do this as auth methods or something like that, and have it as a flag to allow other 

    /// <summary>
    ///     Who provides this account
    /// </summary>
    public enum UserProvider : byte
    {
	    /// <summary>
	    ///     Account details are from Steam
	    /// </summary>
	    Steam,

	    /// <summary>
	    ///     Its an offline account, all details comes from us
	    /// </summary>
	    Offline
    }
}