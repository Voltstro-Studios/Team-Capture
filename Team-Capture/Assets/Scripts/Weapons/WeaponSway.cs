using UnityEngine;

namespace Weapons
{
	public class WeaponSway : MonoBehaviour
	{
		public float xSwayAmount = 0.1f;
		public float ySwayAmount = 0.05f;

		public float maxXAmount = 0.35f;
		public float maxYAmount = 0.2f;

		private Vector3 localPosition;

		public float smooth = 3.0f;

		private void Start()
		{
			localPosition = transform.localPosition;

			//TODO: Setup weapon sway variables based on user settings
		}

		private void Update()
		{
			float fx = -Input.GetAxis("Mouse X") * xSwayAmount;
			float fy = -Input.GetAxis("Mouse Y") * ySwayAmount;

			fx = Mathf.Clamp(fx, -maxXAmount, maxXAmount);
			fy = Mathf.Clamp(fy, -maxYAmount, maxYAmount);

			Vector3 detection = new Vector3(localPosition.x + fx, localPosition.y + fy, localPosition.z);
			transform.localPosition = Vector3.Lerp(transform.localPosition, detection, Time.deltaTime * smooth);
		}
	}
}
