using Mirror;
using UnityEngine;

namespace Team_Capture.Core.Networking.MessagePack
{
	public struct WeaponShootEffectsTargets : NetworkMessage
	{
		public Vector3[] Targets { get; set; }

		public Vector3[] TargetNormals { get; set; }
	}
}