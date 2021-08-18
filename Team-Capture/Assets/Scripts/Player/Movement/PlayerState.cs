// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	///     The state of this player
	/// </summary>
	public struct PlayerState : NetworkMessage
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 WishDir;

		public float RotationX;
		public float RotationY;

		public bool WishJump;

		public int MoveNum;
		public int Timestamp;

		public override string ToString()
		{
			return
				$"PlayerState Pos:{Position}|Vel:{Velocity}|MoveNum:{MoveNum}|Timestamp:{Timestamp}";
		}

		public static PlayerState Zero =>
			new PlayerState
			{
				Position = Vector3.zero,
				WishDir = Vector3.zero,
				RotationX = 0f,
				RotationY = 0f,
				WishJump = false,
				MoveNum = 0,
				Timestamp = 0
			};

		public static PlayerState Interpolate(PlayerState from, PlayerState to, int clientTick)
		{
			float t = (float) (clientTick - from.Timestamp) / (to.Timestamp - from.Timestamp);
			return new PlayerState
			{
				Position = Vector3.Lerp(from.Position, to.Position, t),
				Velocity = Vector3.Lerp(from.Velocity, to.Velocity, t),
				WishDir = Vector3.Lerp(from.WishDir, to.WishDir, t),
				RotationX = Mathf.Lerp(from.RotationX, to.RotationX, t),
				RotationY = Mathf.Lerp(from.RotationY, to.RotationY, t),
				WishJump = to.WishJump,
				MoveNum = 0,
				Timestamp = 0
			};
		}

		public static PlayerState Extrapolate(PlayerState from, int clientTick)
		{
			int t = clientTick - from.Timestamp;
			return new PlayerState
			{
				Position = from.Position + from.Velocity * t,
				MoveNum = from.MoveNum,
				Timestamp = from.Timestamp
			};
		}
	}
}