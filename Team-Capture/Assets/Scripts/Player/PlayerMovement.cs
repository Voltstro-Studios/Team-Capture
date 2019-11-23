using System.Collections;
using UnityEngine;

namespace Player
{
	public class PlayerMovement : MonoBehaviour
	{
		[SerializeField] private string horizontalInputName = "Horizontal";
		[SerializeField] private string verticalInputName = "Vertical";
		[SerializeField] private float walkSpeed = 12;
		[SerializeField] private float slopeForce = 6;
		[SerializeField] private float slopeForceRayLength = 2;
		[SerializeField] private AnimationCurve jumpFallOff;
		[SerializeField] private float jumpMultiplier = 2;
		[SerializeField] private KeyCode jumpKey = KeyCode.Space;

		private bool isJumping;

		private CharacterController charController;

		private float horizInput;
		private float vertInput;

		private float movementSpeed;

		private void Awake()
		{
			charController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			PlayerMove();
		}

		private void GetAxis()
		{
			horizInput = Input.GetAxis(horizontalInputName);
			vertInput = Input.GetAxis(verticalInputName);
		}

		private void PlayerMove()
		{
			GetAxis();

			Transform transform1 = transform;
			Vector3 forwardMovement = transform1.forward * vertInput;
			Vector3 rightMovement = transform1.right * horizInput;

			charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed);

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if ((vertInput != 0 || horizInput != 0) && OnSlope())
				charController.Move(Time.deltaTime * charController.height / 2 * slopeForce * Vector3.down);
			// ReSharper restore CompareOfFloatsByEqualityOperator
			SetMovementSpeed();
			JumpInput();
		}

		private void SetMovementSpeed()
		{
			movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime);
		}

		private bool OnSlope()
		{
			if (isJumping)
				return false;

			if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
				charController.height / 2 * slopeForceRayLength)) return false;

			return hit.normal != Vector3.up;
		}

		private void JumpInput()
		{
			if (!Input.GetKeyDown(jumpKey)) return;

			if (isJumping)
				return;

			StartCoroutine(JumpEvent());
		}

		private IEnumerator JumpEvent()
		{
			charController.slopeLimit = 90.0f;
			float timeInAir = 0.0f;
			do
			{
				float jumpForce = jumpFallOff.Evaluate(timeInAir);
				if (charController.enabled)
				{
					charController.Move(Time.deltaTime * jumpForce * jumpMultiplier * Vector3.up);
					timeInAir += Time.deltaTime;
				}
				yield return null;
			} while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

			charController.slopeLimit = 45.0f;
			isJumping = false;
		}
	}
}
