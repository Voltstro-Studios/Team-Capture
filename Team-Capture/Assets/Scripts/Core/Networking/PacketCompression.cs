using MessagePack;

namespace Team_Capture.Core.Networking
{
	public static class PacketCompression
	{
		public static byte[] CompressMessage(object message)
		{
			return MessagePackSerializer.Serialize(message);
		}

		public static T ReadMessage<T>(byte[] bytes)
		{
			return MessagePackSerializer.Deserialize<T>(bytes);
		}
	}
}