// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.UserManagement
{
	/// <summary>
	///		Who provides this account
	/// </summary>
	public enum UserProvider : byte
	{
		/// <summary>
		///		Account details are from Steam
		/// </summary>
		Steam,
		
		/// <summary>
		///		Its an offline account, all details comes from us
		/// </summary>
		Offline
	}
}