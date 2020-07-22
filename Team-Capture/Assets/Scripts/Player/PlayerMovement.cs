using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Player.Movement;
using UI;
using UnityEngine;

namespace Player
{
	public class PlayerMovement : NetworkBehaviour
	{
		[Header("Speed")]
		[SerializeField] private float moveSpeed = 11.0f;
		[SerializeField] private float jumpSpeed = 8.0f;
		[SerializeField] private float sideStrafeSpeed = 1.0f;

		[Header("Acceleration")]
		[SerializeField] private float airAcceleration = 2.0f;
		[SerializeField] private float airDeacceleration = 2.0f;
		[SerializeField] private float airControl = 5.0f;

		[SerializeField] private float runAcceleration = 14.0f;
		[SerializeField] private float runDeacceleration = 10.0f;

		[SerializeField] private float sideStrafeAcceleration = 50.0f;

		[Header("Friction/Gravity amount")]
		[SerializeField] private float frictionAmount = 6;
		[SerializeField] private float gravityAmount = 20.0f;

		private Vector3 playerVelocity = Vector3.zero;

		private float verticalMove;
		private float horizontalMove;

		private float rotationX;
		private float rotationY;

		private bool wishToJump;

		/// <summary>
		/// Correction threshold for both player and camera rotation
		/// </summary>
		[SerializeField, Range(0.01f, 5f)] private float rotationCorrectionThreshold = 0.01f;

		[SerializeField] private float correctionThreshold = 0.1f;

		private CharacterController charController;
		private Transform cameraTransform;

		#region Inputs

		private readonly List<PlayerInputs> clientInputs = new List<PlayerInputs>();

		private readonly ConcurrentQueue<PlayerInputs> serverInputs = new ConcurrentQueue<PlayerInputs>();

		private readonly List<PlayerState> predictedInputs = new List<PlayerState>();

		#endregion

		[SyncVar(hook = nameof(SyncState))] private PlayerState playerState;

		private long clientTick = 0;

		private void Start()
		{
			charController = GetComponent<CharacterController>();
			cameraTransform = GetComponent<PlayerSetup>().GetPlayerCamera().transform;
		}

		private void Update()
		{
			if (isServer)
			{
				if(serverInputs.TryDequeue(out PlayerInputs result))
					CharacterDoMove(result);
			}
		}

		private void FixedUpdate()
		{
			if (isLocalPlayer)
				clientTick++;
		}

		private void OnDisable()
		{
			verticalMove = 0;
			horizontalMove = 0;

			rotationX = 0;
			rotationY = 0;

			playerVelocity = Vector3.zero;

			wishToJump = false;
		}

		#region Movement Functions

		private void CharacterDoMove(PlayerInputs input)
		{
			wishToJump = input.WishToJump;
			verticalMove = input.Vertical;
			horizontalMove = input.Horizontal;
			rotationX = input.RotationX;
			rotationY = input.RotationY;

			//Clamp the X rotation
			rotationX = Mathf.Clamp(rotationX, -90, 90);

			if (!ClientUI.IsPauseMenuOpen)
			{
				transform.rotation = Quaternion.Euler(0, rotationY, 0); // Rotates the collider
				cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0); // Rotates the camera
			}

			if (charController.isGrounded)
				GroundMove();
			else if (!charController.isGrounded)
				AirMove();

			//Move the controller
			charController.Move(playerVelocity * Time.deltaTime);

			PlayerState state = new PlayerState
			{
				PlayerTransform = transform.position,
				RotationX = rotationX,
				RotationY = rotationY,
				Timestamp = input.Timestamp
			};

			if (isServer)
			{
				//Send back the state
				playerState = state;

				return;
			}

			predictedInputs.Add(state);
		}

		private void AirMove()
		{
			if (!ClientUI.IsPauseMenuOpen)
			{
				Vector3 wishDirection = new Vector3(horizontalMove, 0, verticalMove);
				wishDirection = transform.TransformDirection(wishDirection);

				float wishSpeed = wishDirection.magnitude;
				wishSpeed *= moveSpeed;

				wishDirection.Normalize();

				float acceleration = Vector3.Dot(playerVelocity, wishDirection) < 0 ? airDeacceleration : airAcceleration;

				//If the player is ONLY strafing left or right

				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (verticalMove == 0 && horizontalMove != 0)
				{
					if (wishSpeed > sideStrafeSpeed)
						wishSpeed = sideStrafeSpeed;
					acceleration = sideStrafeAcceleration;
				}
				// ReSharper restore CompareOfFloatsByEqualityOperator

				Accelerate(wishDirection, wishSpeed, acceleration);
				if (airControl > 0)
					AirControl(wishDirection, wishSpeed);
			}

			//Apply gravity
			playerVelocity.y -= gravityAmount * Time.deltaTime;
		}

		private void AirControl(Vector3 wishDirection, float wishSpeed)
		{
			if (ClientUI.IsPauseMenuOpen)
				return;

			//Can't control movement if not moving forward or backward
			if (Mathf.Abs(verticalMove) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
				return;

			float zSpeed = playerVelocity.y;
			playerVelocity.y = 0;

			//Next two lines are equivalent to idTech's VectorNormalize()
			float speed = playerVelocity.magnitude;
			playerVelocity.Normalize();

			float dot = Vector3.Dot(playerVelocity, wishDirection);
			float k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			//Change direction while slowing down
			if (dot > 0)
			{
				playerVelocity.x = playerVelocity.x * speed + wishDirection.x * k;
				playerVelocity.y = playerVelocity.y * speed + wishDirection.y * k;
				playerVelocity.z = playerVelocity.z * speed + wishDirection.z * k;

				playerVelocity.Normalize();
			}

			playerVelocity.x *= speed;
			playerVelocity.y = zSpeed;
			playerVelocity.z *= speed;
		}

		private void GroundMove()
		{
			//Do not apply frictionAmount if the player is queueing up the next jump
			if (!wishToJump)
				ApplyFriction(1.0f);
			else
				ApplyFriction(0);

			Vector3 wishDirection = new Vector3(horizontalMove, 0, verticalMove);
			wishDirection = transform.TransformDirection(wishDirection);
			wishDirection.Normalize();

			float wishSpeed = wishDirection.magnitude;
			wishSpeed *= moveSpeed;

			Accelerate(wishDirection, wishSpeed, runAcceleration);

			//Reset the gravity velocity
			playerVelocity.y = -gravityAmount * Time.deltaTime;

			if (!wishToJump) return;

			playerVelocity.y = jumpSpeed;
			wishToJump = false;
		}

		private void ApplyFriction(float t)
		{
			Vector3 vec = playerVelocity; //Equivalent to: VectorCopy();

			vec.y = 0.0f;
			float speed = vec.magnitude;
			float drop = 0.0f;

			//Only if the player is on the ground then apply frictionAmount
			if (charController.isGrounded)
			{
				float control = speed < runDeacceleration ? runDeacceleration : speed;
				drop = control * frictionAmount * Time.deltaTime * t;
			}

			float newSpeed = speed - drop;
			if (newSpeed < 0)
				newSpeed = 0;
			if (speed > 0)
				newSpeed /= speed;

			playerVelocity.x *= newSpeed;
			playerVelocity.z *= newSpeed;
		}

		private void Accelerate(Vector3 wishDirection, float wishSpeed, float acceleration)
		{
			float currentSpeed = Vector3.Dot(playerVelocity, wishDirection);
			float addSpeed = wishSpeed - currentSpeed;
			if (addSpeed <= 0)
				return;
			float accelerationSpeed = acceleration * Time.deltaTime * wishSpeed;
			if (accelerationSpeed > addSpeed)
				accelerationSpeed = addSpeed;

			playerVelocity.x += accelerationSpeed * wishDirection.x;
			playerVelocity.z += accelerationSpeed * wishDirection.z;
		}

		#endregion

		private void SyncState(PlayerState oldState, PlayerState newState)
		{
			if(isServer && isLocalPlayer) //We are a host
				return;

			if (isLocalPlayer)
			{
				/*
				//Do reconciliation
				PlayerState predictedState = predictedInputs.FirstOrDefault(x => x.Timestamp == newState.Timestamp);
				if(predictedState == null) return;

				//Player Body
				if (Vector3.Distance(newState.PlayerTransform, predictedState.PlayerTransform) > correctionThreshold)
					transform.position = newState.PlayerTransform;

				//Body Rotation
				if(Quaternion.Angle(
					Quaternion.Euler(0, newState.RotationY, 0),
					Quaternion.Euler(0, predictedState.RotationY, 0)) > rotationCorrectionThreshold)
					transform.rotation = Quaternion.Euler(0, newState.RotationY, 0);

				//Camera Rotation
				if (Quaternion.Angle(Quaternion.Euler(newState.RotationX, newState.RotationY, 0),
					Quaternion.Euler(predictedState.RotationX, predictedState.RotationY, 0)) > rotationCorrectionThreshold)
				{
					cameraTransform.rotation = Quaternion.Euler(newState.RotationX, newState.RotationY, 0);
				}
				*/

				return;
			}

			transform.position = newState.PlayerTransform;
			transform.rotation = Quaternion.Euler(0, newState.RotationY, 0); // Rotates the collider
			cameraTransform.rotation = Quaternion.Euler(newState.RotationX, newState.RotationY, 0); // Rotates the camera
		}

		[Client]
		public void AddInput(PlayerInputs input)
		{
			input.Timestamp = clientTick;
			clientInputs.Add(input);

			CharacterDoMove(input);

			if (clientInputs.Count < 20) return;

			if (isServer && isLocalPlayer)
			{
				serverInputs.Enqueue(input);
				clientInputs.Clear();
				return;
			}

			CmdAddInputs(clientInputs.ToArray());
			clientInputs.Clear();
		}

		[Command]
		public void CmdAddInputs(PlayerInputs[] inputs)
		{
			foreach (PlayerInputs input in inputs)
				serverInputs.Enqueue(input);
		}
	}
}