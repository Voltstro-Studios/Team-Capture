using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
	public class AuthCharInput : MonoBehaviour
	{
		private List<CharacterInput> inputBuffer;
		private AuthoritativeCharacter character;
		private AuthCharPredictor predictor;
		private int currentInput;

		private Vector2 directions;
		private Vector2 lookDirections;
		private bool jumping;

		private void Awake()
		{
			inputBuffer = new List<CharacterInput>();
			character = GetComponent<AuthoritativeCharacter>();
			predictor = GetComponent<AuthCharPredictor>();
		}

		private void Update()
		{
			//if (inputBuffer.Count == 0 && input == Vector2.zero && !jump && lastInputSent.Directions == Vector2.zero)
			//	return;

			CharacterInput charInput = new CharacterInput(directions, lookDirections, jumping, currentInput++);
			predictor.AddInput(charInput);

			inputBuffer.Add(charInput);
		}

		private void FixedUpdate()
		{
			if (inputBuffer.Count < character.InputBufferSize)
				return;

			character.CmdMove(inputBuffer.ToArray());
			inputBuffer.Clear();
		}

		public void AddInput(float x, float y, float mouseX, float mouseY, bool jump)
		{
			directions = new Vector2(x, y);
			lookDirections = new Vector2(mouseX, mouseY);
			jumping = jump;
		}
	}
}