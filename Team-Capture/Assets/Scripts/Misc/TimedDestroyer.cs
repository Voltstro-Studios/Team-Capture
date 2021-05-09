using UnityEngine;

namespace Team_Capture.Misc
{
	/// <summary>
	///     Destroys a <see cref="GameObject"/> after a certain amount of time
	/// </summary>
	public class TimedDestroyer : MonoBehaviour
	{
		/// <summary>
		///     What delay to use until destroy
		/// </summary>
		public float destroyDelayTime = 2.0f;

		private void Start()
		{
			Destroy(gameObject, destroyDelayTime);
		}
	}
}