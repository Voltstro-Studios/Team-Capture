// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	/// <summary>
	///     Handles observing other players
	/// </summary>
	internal sealed class PlayerMovementObserver : PlayerMovementStateHandler
	{
		private PlayerMovementManager character;
		private int clientTick;
		private LinkedList<PlayerState> stateBuffer;

		private void Awake()
		{
			character = GetComponent<PlayerMovementManager>();
			stateBuffer = new LinkedList<PlayerState>();
			SetObservedState(character.State);
			AddState(character.State);
		}

		private void Update()
		{
			int pastTick = clientTick - character.interpolationDelay;
			LinkedListNode<PlayerState> fromNode = stateBuffer.First;
			LinkedListNode<PlayerState> toNode = fromNode.Next;

			while (toNode != null && toNode.Value.Timestamp <= pastTick)
			{
				fromNode = toNode;
				toNode = fromNode.Next;
				stateBuffer.RemoveFirst();
			}

			SetObservedState(toNode != null
				? PlayerState.Interpolate(fromNode.Value, toNode.Value, pastTick)
				: fromNode.Value);
		}

		private void FixedUpdate()
		{
			clientTick++;
		}

		private void OnDisable()
		{
			stateBuffer.Clear();
		}

		public override void OnStateChange(PlayerState newState)
		{
			clientTick = newState.Timestamp;
			AddState(newState);
		}

		private void AddState(PlayerState state)
		{
			if (stateBuffer.Count > 0 && stateBuffer.Last.Value.Timestamp > state.Timestamp)
				return;

			stateBuffer.AddLast(state);
		}

		private void SetObservedState(PlayerState newState)
		{
			character.SyncState(newState);
		}
	}
}