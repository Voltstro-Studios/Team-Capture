// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

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
		public PlayerInputs(Vector2 moveDirections, Vector2 lookDirections, bool jump, int inputNum)
		{
			MoveDirections = moveDirections;
			LookDirections = lookDirections;
			Jump = jump;
			InputNum = inputNum;
		}

		public Vector2 MoveDirections;
		public Vector2 LookDirections;

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
			writer.WriteVector2(inputs.MoveDirections);
			writer.WriteVector2(inputs.LookDirections);

			writer.WriteBool(inputs.Jump);
			writer.WriteInt(inputs.InputNum);
		}

		public static PlayerInputs ReaderPlayerInputs(this NetworkReader reader)
		{
			return new PlayerInputs(reader.ReadVector2(), reader.ReadVector2(), reader.ReadBool(), reader.ReadInt());
		}
	}
}