using System.Collections.Generic;
using System.Linq;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	///     Helps in predicting movement
	/// </summary>
	internal sealed class PlayerMovementPredictor : PlayerMovementStateHandler
	{
		private PlayerMovementManager character;
		private PlayerState lastServerState = PlayerState.Zero;
		private LinkedList<PlayerInputs> pendingInputs;
		private PlayerState predictedState;

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

		public override void OnStateChange(PlayerState newState)
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

		public void AddInput(PlayerInputs input)
		{
			pendingInputs.AddLast(input);
			ApplyInput(input);
			character.SyncState(predictedState);
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