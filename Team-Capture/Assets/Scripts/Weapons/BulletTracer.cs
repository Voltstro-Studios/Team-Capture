using UnityEngine;

namespace Weapons
{
	/// <summary>
	/// The line thingy that goes were the bullets go that we fake, because it looks
	/// </summary>
	internal class BulletTracer : MonoBehaviour
	{
		/// <summary>
		/// The bullet tracer <see cref="ParticleSystem"/>
		/// </summary>
		[Tooltip("The bullet tracer Particle System")]
		[SerializeField] private ParticleSystem bulletTracer;

		/// <summary>
		/// Plays the bullet tracer particle
		/// </summary>
		/// <param name="target"></param>
		public void Play(Vector3 target)
		{
			transform.LookAt(target);
			bulletTracer.Play();
		}
	}
}