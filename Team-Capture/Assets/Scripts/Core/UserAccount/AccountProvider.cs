// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Core.UserAccount
{
	/// <summary>
	///		Who provides this account
	/// </summary>
	internal enum AccountProvider : byte
	{
		/// <summary>
		///		Its an offline account, all details comes from us
		/// </summary>
		Offline,
		
		/// <summary>
		///		Account details are from Steam
		/// </summary>
		Steam,
		
		/// <summary>
		///		Account details are from Discord
		/// </summary>
		Discord,
		
		/// <summary>
		///		Unknown.
		///		<para>Don't use</para>
		/// </summary>
		Unknown
	}
}