using UnityEngine;

namespace Player
{
	public class PlayerLook : MonoBehaviour
	{
		[SerializeField] private float axisClamp = 90.0f;
		[SerializeField] private float mouseSensitivity = 125f;
		[SerializeField] private string mouseXInput = "Mouse X";
		[SerializeField] private string mouseYInput = "Mouse Y";
		[SerializeField] private Transform playerTransform;

		private float xRotation;

		private void Update()
		{
			float mouseX = Input.GetAxis(mouseXInput) * mouseSensitivity * Time.deltaTime;
			float mouseY = Input.GetAxis(mouseYInput) * mouseSensitivity * Time.deltaTime;

			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, -axisClamp, axisClamp);

			transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
			playerTransform.Rotate(Vector3.up * mouseX);
		}
	}
}