using UI;
using UnityEngine;

namespace Player
{
	public class PlayerMovement : MonoBehaviour
	{
		public float airAcceleration = 2.0f;
		public float airControl = 1.0f;
		public float airDecceleration = 2.0f;

		private CharacterController charController;

		public float forwardMove;
		public float rightMove;

		public float friction = 6;

		public float gravity = 20.0f;
		public bool holdJumpToBhop;
		public float jumpSpeed = 8.0f;

		public float moveSpeed = 7.0f;

		private Vector3 playerVelocity = Vector3.zero;
		public Transform playerView; // Camera

		private float rotX;
		private float rotY;
		public float runAcceleration = 14.0f;
		public float runDeacceleration = 10.0f;
		public float sideStrafeAcceleration = 50.0f;
		public float sideStrafeSpeed = 1.0f;

		private bool wishJump;
		public float xMouseSensitivity = 30.0f;
		public float yMouseSensitivity = 30.0f;

		private void Start()
		{
			charController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			if (!ClientUI.IsPauseMenuOpen)
			{
				rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
				rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;
			}

			// Clamp the X rotation
			rotX = Mathf.Clamp(rotX, -90, 90);

			transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
			playerView.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera

			/* Movement, here's the important part */
			QueueJump();

			if (charController.isGrounded )
				GroundMove();
			else if (!charController.isGrounded)
				AirMove();

			// Move the controller
			charController.Move(playerVelocity * Time.deltaTime);
		}

		private void SetMovementDir()
		{
			if (ClientUI.IsPauseMenuOpen)
			{
				forwardMove = 0;
				rightMove = 0;

				return;
			}

			forwardMove = Input.GetAxisRaw("Vertical");
			rightMove = Input.GetAxisRaw("Horizontal");
		}

		private void QueueJump()
		{
			if(ClientUI.IsPauseMenuOpen)
				return;

			if (holdJumpToBhop)
			{
				wishJump = Input.GetButton("Jump");
				return;
			}

			if (Input.GetButtonDown("Jump") && !wishJump)
				wishJump = true;
			if (Input.GetButtonUp("Jump"))
				wishJump = false;
		}

		private void AirMove()
		{
			float acceleration;

			SetMovementDir();

			if (!ClientUI.IsPauseMenuOpen)
			{
				Vector3 wishDirection = new Vector3(rightMove, 0, forwardMove);
				wishDirection = transform.TransformDirection(wishDirection);

				float wishSpeed = wishDirection.magnitude;
				wishSpeed *= moveSpeed;

				wishDirection.Normalize();

				if (Vector3.Dot(playerVelocity, wishDirection) < 0)
					acceleration = airDecceleration;
				else
					acceleration = airAcceleration;
				// If the player is ONLY strafing left or right
				// ReSharper disable CompareOfFloatsByEqualityOperator
				if (forwardMove == 0 && rightMove != 0)
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

			// Apply gravity
			playerVelocity.y -= gravity * Time.deltaTime;
		}

		private void AirControl(Vector3 wishDirection, float wishSpeed)
		{
			if(ClientUI.IsPauseMenuOpen)
				return;

			// Can't control movement if not moving forward or backward
			if (Mathf.Abs(forwardMove) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
				return;
			float zSpeed = playerVelocity.y;
			playerVelocity.y = 0;
			/* Next two lines are equivalent to idTech's VectorNormalize() */
			float speed = playerVelocity.magnitude;
			playerVelocity.Normalize();

			float dot = Vector3.Dot(playerVelocity, wishDirection);
			float k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			// Change direction while slowing down
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
			// Do not apply friction if the player is queueing up the next jump
			if (!wishJump)
				ApplyFriction(1.0f);
			else
				ApplyFriction(0);

			SetMovementDir();

			Vector3 wishDirection = new Vector3(rightMove, 0, forwardMove);
			wishDirection = transform.TransformDirection(wishDirection);
			wishDirection.Normalize();

			float wishSpeed = wishDirection.magnitude;
			wishSpeed *= moveSpeed;

			Accelerate(wishDirection, wishSpeed, runAcceleration);

			// Reset the gravity velocity
			playerVelocity.y = -gravity * Time.deltaTime;

			if (!wishJump) return;

			playerVelocity.y = jumpSpeed;
			wishJump = false;
		}

		private void ApplyFriction(float t)
		{
			Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();

			vec.y = 0.0f;
			float speed = vec.magnitude;
			float drop = 0.0f;

			/* Only if the player is on the ground then apply friction */
			if (charController.isGrounded)
			{
				float control = speed < runDeacceleration ? runDeacceleration : speed;
				drop = control * friction * Time.deltaTime * t;
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

		private void OnDisable()
		{
			//Reset all values to 0
			forwardMove = 0;
			rightMove = 0;

			rotX = 0;
			rotY = 0;

			wishJump = false;

			playerVelocity = Vector3.zero;
			charController.Move(Vector3.zero);
		}
	}
}