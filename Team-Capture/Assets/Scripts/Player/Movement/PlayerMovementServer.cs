// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	///     Handles processing inputs and performing movements on the server
	/// </summary>
	internal sealed class PlayerMovementServer : MonoBehaviour
	{
		private PlayerMovementManager character;
		private Queue<PlayerInputs> inputBuffer;

		private PlayerInputs lastInputs;
		private int serverTick;

		private void Awake()
		{
			inputBuffer = new Queue<PlayerInputs>();
			character = GetComponent<PlayerMovementManager>();
			character.State = PlayerState.Zero;
		}

		private void FixedUpdate()
		{
			serverTick++;

			PlayerState state = character.State;

			if (inputBuffer.Count != 0)
				lastInputs = inputBuffer.Dequeue();

			state = character.Move(state, lastInputs, serverTick);
			character.SyncState(state);

			state.Position = transform.position;
			character.State = state;
			character.OnServerStateChange(state, state);
		}

		private void OnDisable()
		{
			inputBuffer.Clear();
		}

		/// <summary>
		///     Adds all the inputs and moves
		/// </summary>
		/// <param name="inputs"></param>
		public void AddInputs(PlayerInputs[] inputs)
		{
			foreach (PlayerInputs input in inputs)
				inputBuffer.Enqueue(input);
		}
	}
}