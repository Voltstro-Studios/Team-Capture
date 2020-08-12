using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
	/// <summary>
	/// Handles observing other players
	/// </summary>
	public class AuthCharObserver : MonoBehaviour, IAuthCharStateHandler
	{
		private LinkedList<CharacterState> stateBuffer;
		private AuthoritativeCharacter character;
		private int clientTick;

		private void Awake()
		{
			character = GetComponent<AuthoritativeCharacter>();
			stateBuffer = new LinkedList<CharacterState>();
			SetObservedState(character.state);
			AddState(character.state);
		}

		private void Update()
		{
			int pastTick = clientTick - character.interpolationDelay;
			LinkedListNode<CharacterState> fromNode = stateBuffer.First;
			LinkedListNode<CharacterState> toNode = fromNode.Next;

			while (toNode != null && toNode.Value.Timestamp <= pastTick)
			{
				fromNode = toNode;
				toNode = fromNode.Next;
				stateBuffer.RemoveFirst();
			}

			SetObservedState(toNode != null ? CharacterState.Interpolate(fromNode.Value, toNode.Value, pastTick) : fromNode.Value);
		}

		private void FixedUpdate()
		{
			clientTick++;
		}

		public void OnStateChange(CharacterState newState)
		{
			clientTick = newState.Timestamp;
			AddState(newState);
		}

		private void AddState(CharacterState state)
		{
			if (stateBuffer.Count > 0 && stateBuffer.Last.Value.Timestamp > state.Timestamp)
				return;

			stateBuffer.AddLast(state);
		}

		private void SetObservedState(CharacterState newState)
		{
			character.SyncState(newState);
		}
	}
}