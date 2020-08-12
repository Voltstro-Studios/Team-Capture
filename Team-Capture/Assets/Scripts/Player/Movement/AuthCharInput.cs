using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
	/// <summary>
	/// Handles sending inputs the server for movement
	/// </summary>
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

		private void OnDisable()
		{
			inputBuffer.Clear();

			directions = Vector2.zero;
			lookDirections = Vector2.zero;
			jumping = false;
		}

		private void FixedUpdate()
		{
			CharacterInput charInput = new CharacterInput(directions, lookDirections, jumping, currentInput++);
			predictor.AddInput(charInput);

			inputBuffer.Add(charInput);

			//Don't send input until our buffer is big enough
			if (inputBuffer.Count < character.InputBufferSize)
				return;

			//Tell the server our inputs
			character.CmdMove(inputBuffer.ToArray());
			inputBuffer.Clear();
		}

		/// <summary>
		/// Sets the input to send
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="mouseX"></param>
		/// <param name="mouseY"></param>
		/// <param name="jump"></param>
		public void SetInput(float x, float y, float mouseX, float mouseY, bool jump)
		{
			directions = new Vector2(x, y);
			lookDirections = new Vector2(mouseX, mouseY);
			jumping = jump;
		}
	}
}