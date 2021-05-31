// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Team_Capture.Helper
{
	/// <summary>
	///     Helper for networking
	/// </summary>
	public static class NetHelper
	{
		/// <summary>
		///     Gets the local IP address of this computer
		///     <para>
		///         It will return <c>localhost</c> if no networks are available, or <c>127.0.0.1</c> if there are multiple
		///         interfaces, otherwise the actual IP address
		///     </para>
		/// </summary>
		/// <returns></returns>
		public static string LocalIpAddress()
		{
			//There is no network available, so IDK
			if (!NetworkInterface.GetIsNetworkAvailable())
				return "localhost";

			//Get all the network interfaces
			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			List<NetworkInterface> activeNetworkInterfaces = networkInterfaces.Where(networkInterface =>
				networkInterface.OperationalStatus == OperationalStatus.Up && !networkInterface.IsReceiveOnly).ToList();

			//If there is more then one network interface, default to local host
			if (activeNetworkInterfaces.Count > 1)
				return "127.0.0.1";

			//Get the address
			NetworkInterface activeInterface = activeNetworkInterfaces[0];
			foreach (IPAddressInformation information in activeInterface.GetIPProperties().AnycastAddresses)
				if (information.Address.AddressFamily == AddressFamily.InterNetwork)
					return information.Address.ToString();

			//Fuck Do I know what to do if we hit here
			return "localhost";
		}
	}
}