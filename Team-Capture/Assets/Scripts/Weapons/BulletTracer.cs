using UnityEngine;

namespace Weapons
{
	internal class BulletTracer : MonoBehaviour
	{
		[SerializeField] private ParticleSystem bulletTracer;

		public void Play(Vector3 target)
		{
			transform.LookAt(target);
			bulletTracer.Play();
		}
	}
}