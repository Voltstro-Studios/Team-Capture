using Attributes;
using Mirror;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerManager))]
	public class AuthoritativeCharacter : NetworkBehaviour
	{
		private PlayerManager playerManager;

		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundDistance = 0.7f;
		[SerializeField] private LayerMask groundMask;

		#region Movement Controls

		[Header("Movement Settings")]
		[SerializeField] private float moveSpeed = 11.0f;
		[SerializeField] private float jumpHeight = 3f;

		[SerializeField] private float gravityAmount = 9.81f;

		[SerializeField] private Transform cameraTransform;

		#endregion

		/// <summary>
		/// Delay for interpolation
		/// </summary>
		[Header("Network"), SerializeField, Range(1, 60), Tooltip("Delay for interpolation")]
		public int interpolationDelay = 12;

		/// <summary>
		/// The rate for updating inputs
		/// </summary>
		[SerializeField, Range(10, 50), Tooltip("The rate for updating inputs")]
		private int inputUpdateRate = 10;

		/// <summary>
		/// Controls how many inputs are needed before sending update command
		/// </summary>
		public int InputBufferSize { get; private set; }

		/// <summary>
		/// The current state of the player
		/// </summary>
		[SyncVar(hook = nameof(OnServerStateChange))]
		public CharacterState state = CharacterState.Zero;

		/// <summary>
		/// The state handler (Observer or predictor)
		/// </summary>
		private IAuthCharStateHandler stateHandler;

		/// <summary>
		/// The <see cref="AuthCharServer"/>, set if we are the server
		/// </summary>
		private AuthCharServer server;

		/// <summary>
		/// The <see cref="CharacterController"/>
		/// </summary>
		private CharacterController characterController;

		private void Awake()
		{
			InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;
			playerManager = GetComponent<PlayerManager>();
		}

		private void OnGUI()
		{
			if(!showPos) return;

			GUI.Label(new Rect(10, 10, 1000, 20), $"Velocity: {characterController.velocity}");
			GUI.Label(new Rect(10, 30, 1000, 20), $"Position: {transform.position}");
			GUI.Label(new Rect(10, 50, 1000, 20), $"IsGround: {Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask)}");
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			server = gameObject.AddComponent<AuthCharServer>();
		}

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
			if (!isLocalPlayer)
			{
				stateHandler = gameObject.AddComponent<AuthCharObserver>();
				return;
			}

			//Setup for local player
			GetComponentInChildren<Renderer>().material.color = Color.green;
			stateHandler = gameObject.AddComponent<AuthCharPredictor>();
			gameObject.AddComponent<AuthCharInput>();
		}

		public void SyncState(CharacterState overrideState)
		{
			if(playerManager.IsDead)
				return;

			characterController.Move(overrideState.Position - transform.position);

			transform.rotation = Quaternion.Euler(0, overrideState.RotationY, 0);
			cameraTransform.rotation = Quaternion.Euler(overrideState.RotationX, overrideState.RotationY, 0);
		}

		public void OnServerStateChange(CharacterState oldState, CharacterState newState)
		{
			state = newState;
			stateHandler?.OnStateChange(state);
		}

		[Command(channel = 0)]
		public void CmdMove(CharacterInput[] inputs)
		{
			server.AddInputs(inputs);
		}

		/// <summary>
		/// Sets the player's position
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="rotationX"></param>
		/// <param name="rotationY"></param>
		/// <param name="resetVelocity"></param>
		[Server]
		public void SetCharacterPosition(Vector3 pos, float rotationX, float rotationY, bool resetVelocity = false)
		{
			transform.position = pos;
			transform.rotation = Quaternion.Euler(0, rotationY, 0);
			cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0);

			state.Position = pos;
			state.RotationX = rotationX;
			state.RotationY = rotationY;

			if (resetVelocity)
				state.Velocity = Vector3.zero;
		}

		#region Movement Methods

		public CharacterState Move(CharacterState previous, CharacterInput input, int timestamp)
		{
			CharacterState characterState = new CharacterState
			{
				MoveNum = previous.MoveNum + 1,
				Timestamp = timestamp,
				Position = previous.Position,
				Velocity = previous.Velocity,
				RotationX = previous.RotationX,
				RotationY = previous.RotationY
			};

			bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);

			//Calculate velocity
			Vector3 inputs = new Vector3(input.Directions.x, 0f, input.Directions.y) * moveSpeed;
			characterState.Velocity = transform.TransformDirection(inputs);
			characterState.Velocity.y = previous.Velocity.y;

			//Gravity
			if(!isGrounded)
				characterState.Velocity.y -= gravityAmount * Time.deltaTime;
			else
				characterState.Velocity.y = -2f;

			//Jumping
			if (input.Jump && isGrounded)
				characterState.Velocity.y = jumpHeight;

			//Apply velocity to position
			characterState.Position += characterState.Velocity * Time.deltaTime;

			//Mouse Movement
			characterState.RotationX -= input.MouseDirections.y * 0.02f;
			characterState.RotationY += input.MouseDirections.x * 0.02f;

			characterState.RotationX = Mathf.Clamp(characterState.RotationX, -90, 90);

			return characterState;
		}

		#endregion

		#region Commands

		private static bool showPos;

		[ConCommand("cl_showpos", "Shows info about position data", 1, 1)]
		public static void ShowPosInfo(string[] args)
		{
			string enabled = args[0].ToLower();
			if (bool.TryParse(enabled, out bool result))
			{
				showPos = result;
				return;
			}

			Logger.Error("Invalid input! Needs to be either true or false!");
		}

		#endregion
	}
}