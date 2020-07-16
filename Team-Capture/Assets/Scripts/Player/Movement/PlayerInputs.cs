using Mirror;

namespace Player.Movement
{
	public class PlayerInputs : MessageBase
	{
		public float Vertical;
		public float Horizontal;

		public float RotationX;
		public float RotationY;

		public bool WishToJump;

		public long Timestamp;

		public override void Serialize(NetworkWriter writer)
		{
			writer.WriteSingle(Vertical);
			writer.WriteSingle(Horizontal);

			writer.WriteSingle(RotationX);
			writer.WriteSingle(RotationY);

			writer.WriteBoolean(WishToJump);

			writer.WriteInt64(Timestamp);
		}

		public override void Deserialize(NetworkReader reader)
		{
			Vertical = reader.ReadSingle();
			Horizontal = reader.ReadSingle();

			RotationX = reader.ReadSingle();
			RotationY = reader.ReadSingle();

			WishToJump = reader.ReadBoolean();

			Timestamp = reader.ReadInt64();
		}
	}
}