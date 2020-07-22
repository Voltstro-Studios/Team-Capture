using Mirror;
using UnityEngine;

namespace Player.Movement
{
	/// <summary>
	/// A message from the server to all the clients, telling them what the other player's positions are
	/// </summary>
	public class PlayerState : MessageBase
	{
		public Vector3 PlayerTransform;

		public float RotationX;
		public float RotationY;

		public long Timestamp;

		public override void Serialize(NetworkWriter writer)
		{
			writer.WriteVector3(PlayerTransform);

			writer.WriteSingle(RotationX);
			writer.WriteSingle(RotationY);

			writer.WriteInt64(Timestamp);
		}

		public override void Deserialize(NetworkReader reader)
		{
			PlayerTransform = reader.ReadVector3();

			RotationX = reader.ReadSingle();
			RotationY = reader.ReadSingle();

			Timestamp = reader.ReadInt64();
		}
	}
}