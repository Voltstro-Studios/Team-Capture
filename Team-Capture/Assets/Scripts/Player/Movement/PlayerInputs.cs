using Mirror;
using UnityEngine;
using UnityEngine.Scripting;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	///     The inputs to send to the server
	/// </summary>
	public struct PlayerInputs : NetworkMessage
	{
		public PlayerInputs(Vector2 dirs, Vector2 mouseDirs, bool jump, int inputNum)
		{
			Directions = dirs;
			MouseDirections = mouseDirs;
			Jump = jump;
			InputNum = inputNum;
		}

		public Vector2 Directions;
		public Vector2 MouseDirections;

		public bool Jump;

		public int InputNum;

		public static PlayerInputs Zero =>
			new PlayerInputs(Vector2.zero, Vector2.zero, false, 0);
	}

	[Preserve]
	public static class PlayerInputsReaderWriter
	{
		public static void WritePlayerInputs(this NetworkWriter writer, PlayerInputs inputs)
		{
			writer.WriteVector2(inputs.Directions);
			writer.WriteVector2(inputs.MouseDirections);

			writer.WriteBoolean(inputs.Jump);
			writer.WriteInt32(inputs.InputNum);
		}

		public static PlayerInputs ReaderPlayerInputs(this NetworkReader reader)
		{
			return new PlayerInputs(reader.ReadVector2(), reader.ReadVector2(), reader.ReadBoolean(), reader.ReadInt32());
		}
	}
}