using Mirror;
using Team_Capture.Core.Compression;
using UnityEngine.Scripting;

namespace Team_Capture.Core.UserAccount
{
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