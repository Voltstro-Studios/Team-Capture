// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using Team_Capture.Console;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
	//This code is built on unity-fastpacedmultiplayer
	//https://github.com/JoaoBorks/unity-fastpacedmultiplayer
	//
	//MIT License
	//Copyright (c) 2015 ultimatematchthree, 2017 Joao Borks [joao.borks@gmail.com]

	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerManager))]
	public sealed class PlayerMovementManager : NetworkBehaviour
	{
		#region Commands

		[ConVar("cl_showpos", "Shows the position and other stuff like that of the player", true)]
		public static bool ShowPos = false;

		#endregion

		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundDistance = 0.7f;
		[SerializeField] private LayerMask groundMask;

		/// <summary>
		///     Delay for interpolation
		/// </summary>
		[Header("Network")] [Range(1, 60)] [Tooltip("Delay for interpolation")]
		public int interpolationDelay = 12;

		/// <summary>
		///     The rate for updating inputs
		/// </summary>
		[SerializeField] [Range(10, 50)] [Tooltip("The rate for updating inputs")]
		private int inputUpdateRate = 10;

		/// <summary>
		///     The current state of the player
		/// </summary>
		[SyncVar(hook = nameof(OnServerStateChange))]
		internal PlayerState State = PlayerState.Zero;

		/// <summary>
		///		The <see cref="PlayerManager"/>
		/// </summary>
		private PlayerManager playerManager;

		/// <summary>
		///     The <see cref="CharacterController" />
		/// </summary>
		private CharacterController characterController;

		/// <summary>
		///     The <see cref="PlayerMovementServer" /> (Only set on server)
		/// </summary>
		private PlayerMovementServer server;

		/// <summary>
		///     The state handler (Observer or predictor)
		/// </summary>
		private PlayerMovementStateHandler stateHandler;

		/// <summary>
		///		The <see cref="PlayerMovementInput"/> (Only set on local client)
		/// </summary>
		private PlayerMovementInput playerInput;

		private PlayerCameraRoll cameraRoll;

		/// <summary>
		///     Controls how many inputs are needed before sending update command
		/// </summary>
		public int InputBufferSize { get; private set; }

		#region Unity Events

		private void Awake()
		{
			InputBufferSize = (int) (1 / Time.fixedDeltaTime) / inputUpdateRate;
			playerManager = GetComponent<PlayerManager>();
		}

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
			if (!isLocalPlayer)
				stateHandler = gameObject.AddComponent<PlayerMovementObserver>();
		}
		
		private void OnGUI()
		{
			if (!ShowPos) return;

			GUI.skin.label.fontSize = 20;

			GUI.Box(new Rect(8, 10, 300, 115), "");

			GUI.Label(new Rect(10, 10, 1000, 40), $"Velocity: {State.Velocity}");
			GUI.Label(new Rect(10, 30, 1000, 40), $"WishDir: {State.WishDir}");
			GUI.Label(new Rect(10, 50, 1000, 40), $"Position: {State.Position}");
			GUI.Label(new Rect(10, 70, 1000, 40), $"LookingAt: {cameraTransform.rotation}");
			GUI.Label(new Rect(10, 90, 1000, 40),
				$"IsGround: {Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask)}");
		}
		
		#endregion

		#region Network Methods

		public override void OnStartLocalPlayer()
		{
			//Setup for local player
			stateHandler = gameObject.AddComponent<PlayerMovementPredictor>();
			playerInput = gameObject.AddComponent<PlayerMovementInput>();
			cameraRoll = GetComponent<PlayerSetup>().GetPlayerCamera().gameObject.AddComponent<PlayerCameraRoll>();
			cameraRoll.baseTransform = transform;
		}
		
		public override void OnStartServer()
		{
			base.OnStartServer();
			server = gameObject.AddComponent<PlayerMovementServer>();
		}
		
		#endregion

		/// <summary>
		///		Disables the state handler
		/// </summary>
		public void DisableStateHandling()
		{
			stateHandler.enabled = false;

			if (isLocalPlayer)
				playerInput.enabled = false;
		}

		/// <summary>
		///		Enables the state handler
		/// </summary>
		public void EnableStateHandling()
		{
			stateHandler.enabled = true;

			if (isLocalPlayer)
				playerInput.enabled = true;
		}

		/// <summary>
		///		Syncs the state and moves the <see cref="CharacterController"/>
		/// </summary>
		/// <param name="overrideState"></param>
		internal void SyncState(PlayerState overrideState)
		{
			if (playerManager.IsDead)
				return;

			if(characterController == null)
				return;

			characterController.Move(overrideState.Position - transform.position);

			transform.rotation = Quaternion.Euler(0, overrideState.RotationY, 0);
			cameraTransform.rotation = Quaternion.Euler(overrideState.RotationX, overrideState.RotationY, 0);
			
			if(cameraRoll != null)
				cameraRoll.SetVelocity(overrideState.Velocity);
		}

		internal void OnServerStateChange(PlayerState oldState, PlayerState newState)
		{
			State = newState;
			stateHandler?.OnStateChange(State);
		}

		/// <summary>
		///		Adds <see cref="PlayerInputs"/> to the server's client input buffer
		/// </summary>
		/// <param name="inputs"></param>
		[Command(channel = Channels.Reliable)]
		internal void CmdMove(PlayerInputs[] inputs)
		{
			//TODO: Buffer protection
			
			server.AddInputs(inputs);
		}

		/// <summary>
		///     Sets the player's position
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="rotationX"></param>
		/// <param name="rotationY"></param>
		/// <param name="resetVelocity"></param>
		[Server]
		public void SetCharacterPosition(Vector3 pos, float rotationX, float rotationY, bool resetVelocity = false)
		{
			PlayerState newState = new PlayerState
			{
				Position = pos,
				RotationX = rotationX,
				RotationY = rotationY,
				Velocity = State.Velocity
			};

			if (resetVelocity)
				newState.Velocity = Vector3.zero;

			State = newState;
			OnServerStateChange(State, newState);

			TargetSetPosition(connectionToClient, State);
		}

		[TargetRpc]
		// ReSharper disable once UnusedParameter.Local
		private void TargetSetPosition(NetworkConnection conn, PlayerState newState)
		{
			if(playerManager.IsDead)
				return;

			transform.position = newState.Position;
			SyncState(newState);
		}

		#region Movement Methods

		internal PlayerState Move(PlayerState previous, PlayerInputs input, int timestamp)
		{
			PlayerState playerState = new PlayerState
			{
				MoveNum = previous.MoveNum + 1,
				Timestamp = timestamp,
				Position = previous.Position,
				Velocity = previous.Velocity,
				RotationX = previous.RotationX,
				RotationY = previous.RotationY
			};

			bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
			playerState.WishJump = input.Jump;
			
			if(isGrounded)
				GroundMove(ref playerState, input);
			else
				AirMove(ref playerState, input);

			playerState.Position += playerState.Velocity * Time.deltaTime;

			//Mouse Movement
			playerState.RotationX -= input.LookDirections.y;
			playerState.RotationY += input.LookDirections.x;

			playerState.RotationX = Mathf.Clamp(playerState.RotationX, -90, 90);

			return playerState;
		}

		private void GroundMove(ref PlayerState state, PlayerInputs input)
		{
			//Do not apply friction if the player is queueing up the next jump
			if (!state.WishJump)
				ApplyFriction(ref state, true, 1.0f);
			else
				ApplyFriction(ref state, true, 0f);

			Vector3 wishDir = new Vector3(input.MoveDirections.x, 0f, input.MoveDirections.y);
			wishDir = transform.TransformDirection(wishDir);
			wishDir.Normalize();
			
			state.WishDir = wishDir;

			float wishSpeed = wishDir.magnitude;
			wishSpeed *= moveSpeed;

			Accelerate(ref state, wishDir, wishSpeed, runAcceleration);

			//Reset the gravity velocity
			state.Velocity.y = -gravityAmount * Time.deltaTime;

			if(state.WishJump)
			{
				state.Velocity.y = jumpHeight;
				state.WishJump = false;
			}
		}

		private void AirMove(ref PlayerState state, PlayerInputs input)
		{
			Vector3 wishDir = new Vector3(input.MoveDirections.x, 0f, input.MoveDirections.y);
			wishDir = transform.TransformDirection(wishDir);

			float wishSpeed = wishDir.magnitude;
			wishSpeed *= moveSpeed;

			wishDir.Normalize();
			state.WishDir = wishDir;

			float wishSpeed2 = wishSpeed;
			float accel = Vector3.Dot(state.Velocity, wishDir) < 0 ? airDecceleration : airAcceleration;
			
			//If the player is ONLY strafing left or right
			if(input.MoveDirections.y == 0 && input.MoveDirections.x != 0)
			{
				if(wishSpeed > sideStrafeSpeed)
					wishSpeed = sideStrafeSpeed;
				
				accel = sideStrafeAcceleration;
			}

			Accelerate(ref state, wishDir, wishSpeed, accel);
			if(airControl > 0)
				AirControl(ref state, input, wishDir, wishSpeed2);
			
			state.Velocity.y -= gravityAmount * Time.deltaTime;
		}
		
		private void AirControl(ref PlayerState state, PlayerInputs input, Vector3 wishDir, float wishSpeed)
		{
			//Can't control movement if not moving forward or backward
			if(Mathf.Abs(input.MoveDirections.y) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
				return;
			
			float zSpeed = state.Velocity.y;
			state.Velocity.y = 0;
			
			float speed = state.Velocity.magnitude;
			state.Velocity.Normalize();

			float dot = Vector3.Dot(state.Velocity, wishDir);
			float k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			//Change direction while slowing down
			if (dot > 0)
			{
				state.Velocity.x = state.Velocity.x * speed + wishDir.x * k;
				state.Velocity.y = state.Velocity.y * speed + wishDir.y * k;
				state.Velocity.z = state.Velocity.z * speed + wishDir.z * k;

				state.Velocity.Normalize();
			}

			state.Velocity.x *= speed;
			state.Velocity.y = zSpeed;
			state.Velocity.z *= speed;
		}
		
		private void ApplyFriction(ref PlayerState state, bool isGrounded, float t)
		{
			Vector3 vec = state.Velocity; 
			vec.y = 0.0f;
			
			float speed = vec.magnitude;
			float drop = 0.0f;
			
			if(isGrounded)
			{
				float control = speed < runDeacceleration ? runDeacceleration : speed;
				drop = control * friction * Time.deltaTime * t;
			}

			float newSpeed = speed - drop;
			if(newSpeed < 0)
				newSpeed = 0;
			if(speed > 0)
				newSpeed /= speed;

			state.Velocity.x *= newSpeed;
			state.Velocity.z *= newSpeed;
		}

		private void Accelerate(ref PlayerState state, Vector3 wishDir, float wishSpeed, float accel)
		{
			float currentSpeed = Vector3.Dot(state.Velocity, wishDir);
			float addSpeed = wishSpeed - currentSpeed;
			if(addSpeed <= 0)
				return;
			
			float accelSpeed = accel * Time.deltaTime * wishSpeed;
			if(accelSpeed > addSpeed)
				accelSpeed = addSpeed;

			state.Velocity.x += accelSpeed * wishDir.x;
			state.Velocity.z += accelSpeed * wishDir.z;
		}

		#endregion

		#region Movement Controls

		[Header("Movement Settings")] 
		[SerializeField] private float moveSpeed = 14.0f;
		[SerializeField] private float friction = 6;
		[SerializeField] private float runAcceleration = 20.0f;
		[SerializeField] private float runDeacceleration = 16.0f;
		[SerializeField] private float airAcceleration = 3.0f;
		[SerializeField] private float airDecceleration = 3.0f;
		[SerializeField] private float airControl = 0.6f;
		[SerializeField] private float sideStrafeAcceleration = 50.0f;
		[SerializeField] private float sideStrafeSpeed = 1.5f;
		[SerializeField] private float jumpHeight = 8.0f;
		[SerializeField] private float gravityAmount = 24.0f;
		
		[SerializeField] private Transform cameraTransform;

		#endregion
	}
}