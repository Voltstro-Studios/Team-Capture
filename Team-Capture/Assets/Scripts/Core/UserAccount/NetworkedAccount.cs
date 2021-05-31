// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Core.Compression;
using UnityEngine.Scripting;

namespace Team_Capture.Core.UserAccount
{
    /// <summary>
    ///     A <see cref="Account"/> that can be sent over the network
    /// </summary>
    internal struct NetworkedAccount : NetworkMessage
    {
        public AccountProvider AccountProvider;
        public ulong AccountId;
        public CompressedNetworkString AccountName;
    }

    [Preserve]
    internal static class NetworkedAccountNetwork
    {
        public static void WriteNetworkedAccount(this NetworkWriter writer, NetworkedAccount account)
        {
            writer.WriteByte((byte)account.AccountProvider);
            writer.WriteULong(account.AccountId);
            account.AccountName.Write(writer);
        }

        public static NetworkedAccount ReadNetworkedAccount(this NetworkReader reader)
        {
            return new NetworkedAccount
            {
                AccountProvider = (AccountProvider) reader.ReadByte(),
                AccountId = reader.ReadULong(),
                AccountName = CompressedNetworkString.Read(reader)
            };
        }
    }
}