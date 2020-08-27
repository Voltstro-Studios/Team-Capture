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
	/// Handles observing other players
	/// </summary>
	public class PlayerMovementObserver : MonoBehaviour, IPlayerMovementStateHandler
	{
		private LinkedList<PlayerState> stateBuffer;
		private PlayerMovementManager character;
		private int clientTick;

		private void Awake()
		{
			character = GetComponent<PlayerMovementManager>();
			stateBuffer = new LinkedList<PlayerState>();
			SetObservedState(character.state);
			AddState(character.state);
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

			SetObservedState(toNode != null ? PlayerState.Interpolate(fromNode.Value, toNode.Value, pastTick) : fromNode.Value);
		}

		private void FixedUpdate()
		{
			clientTick++;
		}

		private void OnDisable()
		{
			stateBuffer.Clear();
		}

		public void OnStateChange(PlayerState newState)
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