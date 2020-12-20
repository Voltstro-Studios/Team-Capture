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
	///     Handles sending inputs the server for movement
	/// </summary>
	internal sealed class PlayerMovementInput : MonoBehaviour
	{
		private PlayerMovementManager character;
		private int currentInput;

		private List<PlayerInputs> inputBuffer;

		private PlayerInputs inputs;
		private PlayerMovementPredictor predictor;

		private void Awake()
		{
			inputBuffer = new List<PlayerInputs>();
			character = GetComponent<PlayerMovementManager>();
			predictor = GetComponent<PlayerMovementPredictor>();
			inputs = new PlayerInputs();
		}

		private void Update()
		{
			currentInput++;
			inputs.InputNum = currentInput;

			predictor.AddInput(inputs);

			inputBuffer.Add(inputs);

			//Don't send input until our buffer is big enough
			if (inputBuffer.Count < character.InputBufferSize)
				return;

			//Tell the server our inputs
			character.CmdMove(inputBuffer.ToArray());
			inputBuffer.Clear();
		}

		private void OnDisable()
		{
			inputBuffer.Clear();

			inputs = PlayerInputs.Zero;
			currentInput = 0;
		}

		/// <summary>
		///     Sets the input to send
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="mouseX"></param>
		/// <param name="mouseY"></param>
		/// <param name="jump"></param>
		public void SetInput(float x, float y, float mouseX, float mouseY, bool jump)
		{
			inputs.Directions.x = x;
			inputs.Directions.y = y;

			inputs.MouseDirections.x = mouseX;
			inputs.MouseDirections.y = mouseY;

			inputs.Jump = jump;
		}
	}
}