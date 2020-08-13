using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	/// Handles processing inputs and performing movements on the server
	/// </summary>
	public class AuthCharServer : MonoBehaviour
	{
		private Queue<CharacterInput> inputBuffer;
		private AuthoritativeCharacter character;
		private int serverTick;

		private CharacterInput lastInput;

		private void Awake()
		{
			inputBuffer = new Queue<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			character.state = CharacterState.Zero;
		}

		private void FixedUpdate()
		{
			serverTick++;

			CharacterState state = character.state;

			if (inputBuffer.Count != 0)
				lastInput = inputBuffer.Dequeue();

			state = character.Move(state, lastInput, serverTick);
			character.SyncState(state);

			state.Position = transform.position;
			character.state = state;
			character.OnServerStateChange(state, state);
		}

		/// <summary>
		/// Adds all the inputs and moves
		/// </summary>
		/// <param name="inputs"></param>
		public void AddInputs(CharacterInput[] inputs)
		{
			foreach (CharacterInput input in inputs)
				inputBuffer.Enqueue(input);
		}
	}
}