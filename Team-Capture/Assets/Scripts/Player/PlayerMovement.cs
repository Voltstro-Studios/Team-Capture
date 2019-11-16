using System.Collections;
using Mirror;
using UnityEngine;

namespace Player
{
	public class PlayerMovement : NetworkBehaviour
	{
		[SerializeField] private string horizontalInputName = "Horizontal";
		[SerializeField] private string verticalInputName = "Vertical";
		[SerializeField] private float walkSpeed = 12;
		[SerializeField] private float slopeForce = 6;
		[SerializeField] private float slopeForceRayLength = 2;
		[SerializeField] private AnimationCurve jumpFallOff = null;
		[SerializeField] private float jumpMultiplier = 2;
		[SerializeField] private KeyCode jumpKey = KeyCode.Space;

		private bool _isJumping;

		private CharacterController _charController;

		private float _horizInput;
		private float _vertInput;

		private float _movementSpeed;

		private void Awake()
		{
			_charController = GetComponent<CharacterController>();
		}

		private void Update()
		{
			if(isLocalPlayer)
			{
				PlayerMove();
			}
		}

		private void GetAxis()
		{
			_horizInput = Input.GetAxis(horizontalInputName);
			_vertInput = Input.GetAxis(verticalInputName);
		}

		private void PlayerMove()
		{
			GetAxis();

			Transform transform1 = transform;
			Vector3 forwardMovement = transform1.forward * _vertInput;
			Vector3 rightMovement = transform1.right * _horizInput;


			_charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * _movementSpeed);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if ((_vertInput != 0 || _horizInput != 0) && OnSlope())
				_charController.Move(Time.deltaTime * _charController.height / 2 * slopeForce * Vector3.down);

			SetMovementSpeed();
			JumpInput();
		}

		private void SetMovementSpeed()
		{
			_movementSpeed = Mathf.Lerp(_movementSpeed, walkSpeed, Time.deltaTime);
		}

		private bool OnSlope()
		{
			if (_isJumping)
				return false;

			if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
				_charController.height / 2 * slopeForceRayLength)) return false;

			return hit.normal != Vector3.up;
		}

		private void JumpInput()
		{
			if (!Input.GetKeyDown(jumpKey)) return;

			if (_isJumping)
				return;

			StartCoroutine(JumpEvent());
		}

		private IEnumerator JumpEvent()
		{
			_charController.slopeLimit = 90.0f;
			float timeInAir = 0.0f;
			do
			{
				float jumpForce = jumpFallOff.Evaluate(timeInAir);
				if (_charController.enabled)
				{
					_charController.Move(Time.deltaTime * jumpForce * jumpMultiplier * Vector3.up);
					timeInAir += Time.deltaTime;
				}
				yield return null;
			} while (!_charController.isGrounded && _charController.collisionFlags != CollisionFlags.Above);

			_charController.slopeLimit = 45.0f;
			_isJumping = false;
		}
	}
}
