using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	/// Helps in predicting movement
	/// </summary>
	public class PlayerMovementPredictor : MonoBehaviour, IPlayerMovementStateHandler
	{
		private LinkedList<PlayerInputs> pendingInputs;
		private PlayerMovementManager character;
		private PlayerState predictedState;
		private PlayerState lastServerState = PlayerState.Zero;

		private void Awake()
		{
			pendingInputs = new LinkedList<PlayerInputs>();
			character = GetComponent<PlayerMovementManager>();
		}

		private void OnDisable()
		{
			pendingInputs.Clear();
			predictedState = PlayerState.Zero;
			lastServerState = PlayerState.Zero;
		}

		public void AddInput(PlayerInputs input)
		{
			pendingInputs.AddLast(input);
			ApplyInput(input);    
			character.SyncState(predictedState);
		}

		public void OnStateChange(PlayerState newState)
		{
			if (newState.Timestamp <= lastServerState.Timestamp) return;

			while (pendingInputs.Count > 0 && pendingInputs.First().InputNum <= newState.MoveNum)
			{
				pendingInputs.RemoveFirst();
			}
			predictedState = newState;
			lastServerState = newState;

			UpdatePredictedState();
		}

		private void UpdatePredictedState()
		{
			foreach (PlayerInputs input in pendingInputs)
				ApplyInput(input);

			character.SyncState(predictedState);
		}

		private void ApplyInput(PlayerInputs input)
		{
			predictedState = character.Move(predictedState, input, 0);
		}
	}
}