using UnityEngine;

namespace Structs
{
	[System.Serializable]
	public struct CharacterState
	{
		public Vector3 position;
		public Vector3 velocity;

		public float rotationX;
		public float rotationY;

		public int moveNum;
		public int timestamp;
    
		public override string ToString()
		{
			return 
				$"CharacterState Pos:{position}|Vel:{velocity}|MoveNum:{moveNum}|Timestamp:{timestamp}";
		}

		public static CharacterState Zero =>
			new CharacterState {
				position = Vector3.zero,
				rotationX = 0f,
				rotationY = 0f,
				moveNum = 0,
				timestamp = 0
			};

		public static CharacterState Interpolate(CharacterState from, CharacterState to, int clientTick)
		{
			float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);
			return new CharacterState
			{
				position = Vector3.Lerp(from.position, to.position, t),
				rotationX = Mathf.Lerp(from.rotationX, to.rotationX, t),
				rotationY = Mathf.Lerp(from.rotationY, to.rotationY, t),
				moveNum = 0,
				timestamp = 0
			};
		}

		public static CharacterState Extrapolate(CharacterState from, int clientTick)
		{
			int t = clientTick - from.timestamp;
			return new CharacterState
			{
				position = from.position + from.velocity * t,
				moveNum = from.moveNum,
				timestamp = from.timestamp
			};
		}
	}
}