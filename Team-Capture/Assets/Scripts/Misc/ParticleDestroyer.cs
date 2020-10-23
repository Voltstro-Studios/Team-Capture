using UnityEngine;

namespace Misc
{
	/// <summary>
	/// Destroys a particle after <see cref="destroyDelayTime"/> passes
	/// </summary>
	public class ParticleDestroyer : MonoBehaviour
	{
		[SerializeField] private float destroyDelayTime = 2.0f;

		private void Start()
		{
			Destroy(gameObject, destroyDelayTime);
		}
	}
}
