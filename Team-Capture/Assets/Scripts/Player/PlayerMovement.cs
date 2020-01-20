using UI;
using UnityEngine;

namespace Player
{
	public class PlayerMovement : MonoBehaviour
	{
		[Header("Speed")]
		[SerializeField] private float moveSpeed = 7.0f;
		[SerializeField] private float jumpSpeed = 8.0f;
		[SerializeField] private float sideStrafeSpeed = 1.0f;
		
		[Header("Acceleration")]
		[SerializeField] private float airAcceleration = 2.0f;
		[SerializeField] private float airDeacceleration = 2.0f;
		[SerializeField] private float airControl = 1.0f;

		[SerializeField] private float runAcceleration = 14.0f;
		[SerializeField] private float runDeacceleration = 10.0f;

		[SerializeField] private float sideStrafeAcceleration = 50.0f;

		[Header("Friction/Gravity amount")]
		[SerializeField] private float frictionAmount = 6;
		[SerializeField] private float gravityAmount = 20.0f;

		[Header("Sensitivity")]
		[SerializeField] private float xMouseSensitivity = 30.0f;
		[SerializeField] private float yMouseSensitivity = 30.0f;

		private Vector3 playerVelocity = Vector3.zero;

		private float verticalMove;
		private float horizontalMove;

		private float rotationX;
		private float rotationY;

		public bool WishToJump { get; private set; }

		private CharacterController charController;
		private Transform cameraTransform;

		private void Start()
		{
			charController = GetComponent<CharacterController>();
			cameraTransform = GetComponent<PlayerSetup>().GetPlayerCamera().transform;
		}

		private void Update()
		{
			//Clamp the X rotation
			rotationX = Mathf.Clamp(rotationX, -90, 90);

			transform.rotation = Quaternion.Euler(0, rotationY, 0); // Rotates the collider
			cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0); // Rotates the camera

			//Movement
			QueueJump();

			if (charController.isGrounded)
				GroundMove();
			else if (!charController.isGrounded)
				AirMove();

			//Move the controller
			charController.Move(playerVelocity * Time.deltaTime);
		}

		private void QueueJump()
		{
			
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
			//TODO: Shouldn't the server handle the gravity?
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
			if (!WishToJump)
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

			if (!WishToJump) return;

			playerVelocity.y = jumpSpeed;
			WishToJump = false;
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

		public void SetMovementDir(float verticalAxis, float horizontalAxis)
		{
			verticalMove = verticalAxis;
			horizontalMove = horizontalAxis;
		}

		public void SetWishJump(bool wishToJump)
		{
			WishToJump = wishToJump;
		}

		public void SetMouseRotation(float x, float y)
		{
			rotationX -= x * xMouseSensitivity * 0.02f;
			rotationY += y * yMouseSensitivity * 0.02f;
		}
	}
}