using UnityEngine;

namespace Misc
{
	public class ParticleDestroyer : MonoBehaviour
	{
		[SerializeField] private float destroyDelayTime = 2.0f;

		private void Start()
		{
			Destroy(gameObject, destroyDelayTime);
		}
	}
}
