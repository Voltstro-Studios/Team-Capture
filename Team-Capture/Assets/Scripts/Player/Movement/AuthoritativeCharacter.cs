using Mirror;
using UnityEngine;

namespace Player.Movement
{
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

		[Header("Network")]
		[SerializeField, Range(1, 60), Tooltip("In steps per second")]  
		public int interpolationDelay = 12;

		[SerializeField, Range(10, 50), Tooltip("In steps per second")]
		private int inputUpdateRate = 10;

		/// <summary>
		/// Controls how many inputs are needed before sending update command
		/// </summary>
		public int InputBufferSize { get; private set; }

		[SyncVar(hook = nameof(OnServerStateChange))]
		public CharacterState state = CharacterState.Zero;

		private CharacterState workingState;
    
		private IAuthCharStateHandler stateHandler;
		private AuthCharServer server;

		private CharacterController characterController;

		private void Awake()
		{
			InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;
			playerManager = GetComponent<PlayerManager>();
		}

		private void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 1000, 20), state.velocity.ToString());
			GUI.Label(new Rect(10, 30, 1000, 20), transform.position.ToString());
			GUI.Label(new Rect(10, 50, 1000, 20), $"Is Ground: {Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask)}");
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

			characterController.Move(overrideState.position - transform.position);

			transform.rotation = Quaternion.Euler(0, overrideState.rotationY, 0);
			cameraTransform.rotation = Quaternion.Euler(overrideState.rotationX, overrideState.rotationY, 0);
		}

		public void OnServerStateChange(CharacterState oldState, CharacterState newState)
		{
			state = newState;
			stateHandler?.OnStateChange(state);
		}

		[Command(channel = 0)]
		public void CmdMove(CharacterInput[] inputs)
		{
			server.Move(inputs);
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

			state.position = pos;
			state.rotationX = rotationX;
			state.rotationY = rotationY;

			if (resetVelocity)
				state.velocity = Vector3.zero;
		}

		#region Movement Methods

		public CharacterState Move(CharacterState previous, CharacterInput input, int timestamp)
		{
			CharacterState characterState = new CharacterState
			{
				moveNum = previous.moveNum + 1,
				timestamp = timestamp,
				position = previous.position,
				velocity = previous.velocity,
				rotationX = previous.rotationX,
				rotationY = previous.rotationY
			};

			bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);

			//Calculate velocity
			Vector3 inputs = new Vector3(input.Directions.x, 0f, input.Directions.y) * moveSpeed;
			characterState.velocity = transform.TransformDirection(inputs);
			characterState.velocity.y = previous.velocity.y;

			//Gravity
			if(!isGrounded)
				characterState.velocity.y -= gravityAmount * Time.deltaTime;
			else
				characterState.velocity.y = -2f;

			//Jumping
			if (input.Jump && isGrounded)
				characterState.velocity.y = jumpHeight;

			//Apply velocity to position
			characterState.position += characterState.velocity * Time.deltaTime;

			//Mouse Movement
			characterState.rotationX -= input.MouseDirections.y * 0.02f;
			characterState.rotationY += input.MouseDirections.x * 0.02f;

			characterState.rotationX = Mathf.Clamp(characterState.rotationX, -90, 90);

			return characterState;
		}

		#endregion
	}
}