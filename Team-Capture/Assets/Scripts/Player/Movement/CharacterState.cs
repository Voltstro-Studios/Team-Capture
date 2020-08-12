using UnityEngine;

namespace Player.Movement
{
	/// <summary>
	/// The state of this player
	/// </summary>
	public struct CharacterState
	{
		public Vector3 Position;
		public Vector3 Velocity;

		public float RotationX;
		public float RotationY;

		public int MoveNum;
		public int Timestamp;
    
		public override string ToString()
		{
			return 
				$"CharacterState Pos:{Position}|Vel:{Velocity}|MoveNum:{MoveNum}|Timestamp:{Timestamp}";
		}

		public static CharacterState Zero =>
			new CharacterState {
				Position = Vector3.zero,
				RotationX = 0f,
				RotationY = 0f,
				MoveNum = 0,
				Timestamp = 0
			};

		public static CharacterState Interpolate(CharacterState from, CharacterState to, int clientTick)
		{
			float t = ((float)(clientTick - from.Timestamp)) / (to.Timestamp - from.Timestamp);
			return new CharacterState
			{
				Position = Vector3.Lerp(from.Position, to.Position, t),
				RotationX = Mathf.Lerp(from.RotationX, to.RotationX, t),
				RotationY = Mathf.Lerp(from.RotationY, to.RotationY, t),
				MoveNum = 0,
				Timestamp = 0
			};
		}

		public static CharacterState Extrapolate(CharacterState from, int clientTick)
		{
			int t = clientTick - from.Timestamp;
			return new CharacterState
			{
				Position = from.Position + from.Velocity * t,
				MoveNum = from.MoveNum,
				Timestamp = from.Timestamp
			};
		}
	}
}