using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Structs;
using UnityEngine;

namespace Networking
{
	public class AuthCharPredictor : MonoBehaviour, IAuthCharStateHandler
	{
		private LinkedList<CharacterInput> pendingInputs;
		private AuthoritativeCharacter character;
		private CharacterState predictedState;
		private CharacterState lastServerState = CharacterState.Zero;

		private void Awake()
		{
			pendingInputs = new LinkedList<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
		}

		public void AddInput(CharacterInput input)
		{
			pendingInputs.AddLast(input);
			ApplyInput(input);    
			character.SyncState(predictedState);
		}

		public void OnStateChange(CharacterState newState)
		{
			if (newState.timestamp <= lastServerState.timestamp) return;

			while (pendingInputs.Count > 0 && pendingInputs.First().InputNum <= newState.moveNum)
			{
				pendingInputs.RemoveFirst();
			}
			predictedState = newState;
			lastServerState = newState;

			UpdatePredictedState();
		}

		private void UpdatePredictedState()
		{
			foreach (CharacterInput input in pendingInputs)
				ApplyInput(input);

			character.SyncState(predictedState);
		}

		private void ApplyInput(CharacterInput input)
		{
			predictedState = character.Move(predictedState, input, 0);
		}
	}
}